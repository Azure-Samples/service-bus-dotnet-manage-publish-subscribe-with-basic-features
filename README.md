---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
  services: Service-Bus
  platforms: dotnet
---

# Getting started on managing Service Bus Publish-Subscribe with basic features in C# #

 Azure Service Bus basic scenario sample.
 - Create namespace.
 - Create a topic.
 - Update topic with new size and a new ServiceBus subscription.
 - Create another ServiceBus subscription in the topic.
 - List topic
 - List ServiceBus subscriptions
 - Get default authorization rule.
 - Regenerate the keys in the authorization rule.
 - Delete one ServiceBus subscription as part of update of topic.
 - Delete another ServiceBus subscription.
 - Delete topic
 - Delete namespace

 For more on how to use Azure Service Bus see the [samples for sending and receiving messages](https://docs.microsoft.com/samples/azure/azure-sdk-for-net/azuremessagingservicebus-samples/).

## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/service-bus-dotnet-manage-publish-subscribe-with-basic-features.git

    cd service-bus-dotnet-manage-publish-subscribe-with-basic-features

    dotnet build

    bin\Debug\net452\ServiceBusPublishSubscribeBasic.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.