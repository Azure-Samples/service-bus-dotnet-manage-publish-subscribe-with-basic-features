// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.ServiceBus;
using Azure.ResourceManager.ServiceBus.Models;

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
        private static ResourceIdentifier? _resourceGroupId = null;
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                //============================================================

                // Create a namespace.
               
                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the USWest region
                var rgName = Utilities.CreateRandomName("rgSB02_");
                Utilities.Log("Creating resource group with name : " + rgName );
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.WestUS));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created resource group with name: " + resourceGroup.Data.Name + "...");

                //create namespace and wait for completion
                var nameSpaceName = Utilities.CreateRandomName("nameSpace");
                Utilities.Log("Creating namespace " + nameSpaceName + " in resource group " + rgName + "...");
                var namespaceCollection = resourceGroup.GetServiceBusNamespaces();
                var data = new ServiceBusNamespaceData(AzureLocation.WestUS)
                {
                    Sku = new ServiceBusSku(ServiceBusSkuName.Standard),
                    Location = AzureLocation.WestUS
                };
                var serviceBusNamespace = (await namespaceCollection.CreateOrUpdateAsync(WaitUntil.Completed, nameSpaceName, data)).Value;
                Utilities.Log("Created service bus " + serviceBusNamespace.Data.Name);

                //============================================================
              
                // Create a topic in namespace
                var topicName = Utilities.CreateRandomName("topic_");
                Utilities.Log("Creating topic " + topicName + " in namespace " + nameSpaceName + "...");
                var topicCollection = serviceBusNamespace.GetServiceBusTopics();
                var topicData = new ServiceBusTopicData()
                {
                    MaxSizeInMegabytes = 2048,
                };
                var topic = (await topicCollection.CreateOrUpdateAsync(WaitUntil.Completed, topicName, topicData)).Value;
                Utilities.Log("Created topic in namespace with name : " +topic.Data.Name);

                //============================================================

                // Get and update topic with new size and a subscription
                Utilities.Log("Updating topic " + topicName + " with new size and a subscription...");
                var getTopic = (serviceBusNamespace.GetServiceBusTopic(topicName)).Value;
                var updateData = new ServiceBusTopicData()
                {
                    MaxSizeInMegabytes = 3072,
                };
                _ = await getTopic.UpdateAsync(WaitUntil.Completed, updateData);

                // Create a service bus subscription in the topic.
                var subscription1Name = Utilities.CreateRandomName("subs1_");
                Utilities.Log("Creating subscription " + subscription1Name + " in topic " + topic.Data.Name + "...");
                var subscription1Collection = topic.GetServiceBusSubscriptions();
                var subscription1Data = new ServiceBusSubscriptionData()
                {
                    RequiresSession = true,
                };
                var subscription1 = (await subscription1Collection.CreateOrUpdateAsync(WaitUntil.Completed, subscription1Name, subscription1Data)).Value;
                Utilities.Log("Created subscription " + subscription1.Data.Name + " in topic " + topic.Data.Name + "...");
                Utilities.Log("Updated topic to change its size in MB along with a subscription");

                //============================================================
                
                // Create a subscription
                var subscription2Name = Utilities.CreateRandomName("subs2_");
                Utilities.Log("Adding second subscription" + subscription2Name + " to topic " + topicName + "...");
                var subscription2Data = new ServiceBusSubscriptionData()
                {
                    RequiresSession = true,
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(10),
                };
                var subscription2 = (await subscription1Collection.CreateOrUpdateAsync(WaitUntil.Completed, subscription2Name, subscription2Data)).Value;
                Utilities.Log("Added second subscription" + subscription2.Data.Name + " to topic " + topicName + "...");

                //=============================================================

                // List topics in namespaces
                var topics = serviceBusNamespace.GetServiceBusTopics().ToList();
                Utilities.Log("Number of topics in namespace :" + topics.Count());

                //=============================================================

                // List all subscriptions for topic in namespaces
                var subscriptions = topic.GetServiceBusSubscriptions().ToList();
                Utilities.Log("Number of subscriptions to topic: " + subscriptions.Count());

                //=============================================================

                // Get connection string for default authorization rule of namespace
                var namespaceAuthorizationRules = serviceBusNamespace.GetServiceBusNamespaceAuthorizationRules().ToList();
                Utilities.Log("Number of authorization rule for namespace :" + namespaceAuthorizationRules.Count());
               
                //=============================================================

                // Delete a topic and namespace
                Utilities.Log("Deleting subscription " + subscription1Name + " in topic " + topicName + " via update flow...");
                _ = await topic.GetServiceBusSubscription(subscription1Name).Value.DeleteAsync(WaitUntil.Completed);
                Utilities.Log("Deleted subscription " + subscription1Name);
                Utilities.Log("Number of subscriptions in the topic after deleting first subscription: " + topic.GetServiceBusSubscriptions().ToList().Count);
                Utilities.Log("Deleting namespace " + nameSpaceName + "...");
                try
                {
                    _ = serviceBusNamespace.DeleteAsync(WaitUntil.Completed);
                }
                catch (Exception)
                {
                }
                Utilities.Log("Deleted namespace " + nameSpaceName);
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);
                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}

