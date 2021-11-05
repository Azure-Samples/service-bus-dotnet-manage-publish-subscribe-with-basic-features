// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.


using System;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent.Models;

namespace ServiceBusPublishSubscribeBasic
{
    public class Program
    {
        /**
         * Azure Service Bus basic scenario sample.
         * - Create namespace.
         * - Create a topic.
         * - Update topic with new size and a new ServiceBus subscription.
         * - Create another ServiceBus subscription in the topic.
         * - List topic
         * - List ServiceBus subscriptions
         * - Get default authorization rule.
         * - Regenerate the keys in the authorization rule.
         * - Delete one ServiceBus subscription as part of update of topic.
         * - Delete another ServiceBus subscription.
         * - Delete topic
         * - Delete namespace
         */
        public static void RunSample(IAzure azure)
        {
            var rgName = SdkContext.RandomResourceName("rgSB02_", 24);
            var namespaceName = SdkContext.RandomResourceName("namespace", 20);
            var topicName = SdkContext.RandomResourceName("topic_", 24);
            var subscription1Name = SdkContext.RandomResourceName("sub1_", 24);
            var subscription2Name = SdkContext.RandomResourceName("sub2_", 24);
            try
            {
                //============================================================
                // Create a namespace.

                Console.WriteLine("Creating name space " + namespaceName + " in resource group " + rgName + "...");

                var serviceBusNamespace = azure.ServiceBusNamespaces
                        .Define(namespaceName)
                        .WithRegion(Region.USWest)
                        .WithNewResourceGroup(rgName)
                        .WithSku(NamespaceSku.Standard)
                        .Create();

                Console.WriteLine("Created service bus " + serviceBusNamespace.Name);
                PrintNamespace(serviceBusNamespace);

                //============================================================
                // Create a topic in namespace

                Console.WriteLine("Creating topic " + topicName + " in namespace " + namespaceName + "...");

                var topic = serviceBusNamespace.Topics.Define(topicName)
                        .WithSizeInMB(2048)
                        .Create();

                Console.WriteLine("Created second queue in namespace");

                PrintTopic(topic);

                //============================================================
                // Get and update topic with new size and a subscription
                Console.WriteLine("Updating topic " + topicName + " with new size and a subscription...");
                topic = serviceBusNamespace.Topics.GetByName(topicName);
                topic = topic.Update()
                        .WithNewSubscription(subscription1Name)
                        .WithSizeInMB(3072)
                        .Apply();

                Console.WriteLine("Updated topic to change its size in MB along with a subscription");

                PrintTopic(topic);

                var firstSubscription = topic.Subscriptions.GetByName(subscription1Name);
                PrintSubscription(firstSubscription);
                //============================================================
                // Create a subscription
                Console.WriteLine("Adding second subscription" + subscription2Name + " to topic " + topicName + "...");
                var secondSubscription = topic.Subscriptions.Define(subscription2Name).WithDeleteOnIdleDurationInMinutes(10).Create();
                Console.WriteLine("Added second subscription" + subscription2Name + " to topic " + topicName + "...");

                PrintSubscription(secondSubscription);

                //=============================================================
                // List topics in namespaces

                var topics = serviceBusNamespace.Topics.List();
                Console.WriteLine("Number of topics in namespace :" + topics.Count());

                foreach (var topicInNamespace  in  topics)
                {
                    PrintTopic(topicInNamespace);
                }

                //=============================================================
                // List all subscriptions for topic in namespaces

                var subscriptions = topic.Subscriptions.List();
                Console.WriteLine("Number of subscriptions to topic: " + subscriptions.Count());

                foreach (var subscription  in  subscriptions)
                {
                    PrintSubscription(subscription);
                }

                //=============================================================
                // Get connection string for default authorization rule of namespace

                var namespaceAuthorizationRules = serviceBusNamespace.AuthorizationRules.List();
                Console.WriteLine("Number of authorization rule for namespace :" + namespaceAuthorizationRules.Count());


                foreach (var namespaceAuthorizationRule in  namespaceAuthorizationRules)
                {
                    PrintNamespaceAuthorizationRule(namespaceAuthorizationRule);
                }

                Console.WriteLine("Getting keys for authorization rule ...");

                var keys = namespaceAuthorizationRules.FirstOrDefault().GetKeys();
                PrintKeys(keys);
                Console.WriteLine("Regenerating secondary key for authorization rule ...");
                keys = namespaceAuthorizationRules.FirstOrDefault().RegenerateKey(Policykey.SecondaryKey);
                PrintKeys(keys);

                //=============================================================
                // Delete a queue and namespace
                Console.WriteLine("Deleting subscription " + subscription1Name + " in topic " + topicName + " via update flow...");
                topic = topic.Update().WithoutSubscription(subscription1Name).Apply();
                Console.WriteLine("Deleted subscription " + subscription1Name + "...");

                Console.WriteLine("Number of subscriptions in the topic after deleting first subscription: " + topic.SubscriptionCount);

                Console.WriteLine("Deleting namespace " + namespaceName + "...");
                // This will delete the namespace and queue within it.
                try
                {
                    azure.ServiceBusNamespaces.DeleteById(serviceBusNamespace.Id);
                }
                catch (Exception)
                {
                }
                Console.WriteLine("Deleted namespace " + namespaceName + "...");
            }
            finally
            {
                try
                {
                    Console.WriteLine("Deleting Resource Group: " + rgName);
                    azure.ResourceGroups.BeginDeleteByName(rgName);
                    Console.WriteLine("Deleted Resource Group: " + rgName);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Console.WriteLine(g);
                }
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

                var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

                // Print selected subscription
                Console.WriteLine("Selected subscription: " + azure.SubscriptionId);

                RunSample(azure);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void PrintNamespace(IServiceBusNamespace serviceBusNamespace)
        {
            var builder = new StringBuilder()
                    .Append("Service bus Namespace: ").Append(serviceBusNamespace.Id)
                    .Append("\n\tName: ").Append(serviceBusNamespace.Name)
                    .Append("\n\tRegion: ").Append(serviceBusNamespace.RegionName)
                    .Append("\n\tResourceGroupName: ").Append(serviceBusNamespace.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(serviceBusNamespace.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(serviceBusNamespace.UpdatedAt)
                    .Append("\n\tDnsLabel: ").Append(serviceBusNamespace.DnsLabel)
                    .Append("\n\tFQDN: ").Append(serviceBusNamespace.Fqdn)
                    .Append("\n\tSku: ")
                    .Append("\n\t\tCapacity: ").Append(serviceBusNamespace.Sku.Capacity)
                    .Append("\n\t\tSkuName: ").Append(serviceBusNamespace.Sku.Name)
                    .Append("\n\t\tTier: ").Append(serviceBusNamespace.Sku.Tier);

            Console.WriteLine(builder.ToString());
        }

        static void PrintTopic(ITopic topic)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Service bus topic: ").Append(topic.Id)
                    .Append("\n\tName: ").Append(topic.Name)
                    .Append("\n\tResourceGroupName: ").Append(topic.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(topic.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(topic.UpdatedAt)
                    .Append("\n\tAccessedAt: ").Append(topic.AccessedAt)
                    .Append("\n\tActiveMessageCount: ").Append(topic.ActiveMessageCount)
                    .Append("\n\tCurrentSizeInBytes: ").Append(topic.CurrentSizeInBytes)
                    .Append("\n\tDeadLetterMessageCount: ").Append(topic.DeadLetterMessageCount)
                    .Append("\n\tDefaultMessageTtlDuration: ").Append(topic.DefaultMessageTtlDuration)
                    .Append("\n\tDuplicateMessageDetectionHistoryDuration: ").Append(topic.DuplicateMessageDetectionHistoryDuration)
                    .Append("\n\tIsBatchedOperationsEnabled: ").Append(topic.IsBatchedOperationsEnabled)
                    .Append("\n\tIsDuplicateDetectionEnabled: ").Append(topic.IsDuplicateDetectionEnabled)
                    .Append("\n\tIsExpressEnabled: ").Append(topic.IsExpressEnabled)
                    .Append("\n\tIsPartitioningEnabled: ").Append(topic.IsPartitioningEnabled)
                    .Append("\n\tDeleteOnIdleDurationInMinutes: ").Append(topic.DeleteOnIdleDurationInMinutes)
                    .Append("\n\tMaxSizeInMB: ").Append(topic.MaxSizeInMB)
                    .Append("\n\tScheduledMessageCount: ").Append(topic.ScheduledMessageCount)
                    .Append("\n\tStatus: ").Append(topic.Status)
                    .Append("\n\tTransferMessageCount: ").Append(topic.TransferMessageCount)
                    .Append("\n\tSubscriptionCount: ").Append(topic.SubscriptionCount)
                    .Append("\n\tTransferDeadLetterMessageCount: ").Append(topic.TransferDeadLetterMessageCount);

            Console.WriteLine(builder.ToString());
        }

        static void PrintSubscription(Microsoft.Azure.Management.ServiceBus.Fluent.ISubscription serviceBusSubscription)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Service bus subscription: ").Append(serviceBusSubscription.Id)
                    .Append("\n\tName: ").Append(serviceBusSubscription.Name)
                    .Append("\n\tResourceGroupName: ").Append(serviceBusSubscription.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(serviceBusSubscription.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(serviceBusSubscription.UpdatedAt)
                    .Append("\n\tAccessedAt: ").Append(serviceBusSubscription.AccessedAt)
                    .Append("\n\tActiveMessageCount: ").Append(serviceBusSubscription.ActiveMessageCount)
                    .Append("\n\tDeadLetterMessageCount: ").Append(serviceBusSubscription.DeadLetterMessageCount)
                    .Append("\n\tDefaultMessageTtlDuration: ").Append(serviceBusSubscription.DefaultMessageTtlDuration)
                    .Append("\n\tIsBatchedOperationsEnabled: ").Append(serviceBusSubscription.IsBatchedOperationsEnabled)
                    .Append("\n\tDeleteOnIdleDurationInMinutes: ").Append(serviceBusSubscription.DeleteOnIdleDurationInMinutes)
                    .Append("\n\tScheduledMessageCount: ").Append(serviceBusSubscription.ScheduledMessageCount)
                    .Append("\n\tStatus: ").Append(serviceBusSubscription.Status)
                    .Append("\n\tTransferMessageCount: ").Append(serviceBusSubscription.TransferMessageCount)
                    .Append("\n\tIsDeadLetteringEnabledForExpiredMessages: ").Append(serviceBusSubscription.IsDeadLetteringEnabledForExpiredMessages)
                    .Append("\n\tIsSessionEnabled: ").Append(serviceBusSubscription.IsSessionEnabled)
                    .Append("\n\tLockDurationInSeconds: ").Append(serviceBusSubscription.LockDurationInSeconds)
                    .Append("\n\tMaxDeliveryCountBeforeDeadLetteringMessage: ").Append(serviceBusSubscription.MaxDeliveryCountBeforeDeadLetteringMessage)
                    .Append("\n\tIsDeadLetteringEnabledForFilterEvaluationFailedMessages: ").Append(serviceBusSubscription.IsDeadLetteringEnabledForFilterEvaluationFailedMessages)
                    .Append("\n\tTransferMessageCount: ").Append(serviceBusSubscription.TransferMessageCount)
                    .Append("\n\tTransferDeadLetterMessageCount: ").Append(serviceBusSubscription.TransferDeadLetterMessageCount);

            Console.WriteLine(builder.ToString());
        }

        static void PrintNamespaceAuthorizationRule(INamespaceAuthorizationRule namespaceAuthorizationRule)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Service bus queue authorization rule: ").Append(namespaceAuthorizationRule.Id)
                    .Append("\n\tName: ").Append(namespaceAuthorizationRule.Name)
                    .Append("\n\tResourceGroupName: ").Append(namespaceAuthorizationRule.ResourceGroupName)
                    .Append("\n\tNamespace Name: ").Append(namespaceAuthorizationRule.NamespaceName);

            var rights = namespaceAuthorizationRule.Rights;
            builder.Append("\n\tNumber of access rights in queue: ").Append(rights.Count());
            foreach (var right in rights)
            {
                builder.Append("\n\t\tAccessRight: ")
                        .Append("\n\t\t\tName :").Append(right.ToString());
            }

            Console.WriteLine(builder.ToString());
        }

        static void PrintKeys(IAuthorizationKeys keys)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Authorization keys: ")
                    .Append("\n\tPrimaryKey: ").Append(keys.PrimaryKey)
                    .Append("\n\tPrimaryConnectionString: ").Append(keys.PrimaryConnectionString)
                    .Append("\n\tSecondaryKey: ").Append(keys.SecondaryKey)
                    .Append("\n\tSecondaryConnectionString: ").Append(keys.SecondaryConnectionString);

            Console.WriteLine(builder.ToString());
        }
    }
}
