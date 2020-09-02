---
page_type: sample
languages:
  - csharp
products:
  - azure
  - dotnet
  - azure-service-bus
description: "This sample shows how to manage Service Bus manage Publish-Subscribe with basic features."
urlFragment: service-bus-dotnet-manage-publish-subscribe-with-basic-features
---

# Getting started on managing Service Bus Publish-Subscribe with basic features in C#

## Prerequisites

To complete this tutorial:

If you don't have an Azure subscription, create a [free account] before you begin.

### Create an auth file

This project requires a auth file to be stored in an environment variable securely on the machine running the sample. You can generate this file using [Azure CLI 2.0] through the following command. Making sure you set your subscription so that you have the privileges to create Service Bus.

```azure-cli
az login
az account set --subscription "<YourSubscriptionId>"
az ad sp create-for-rbac --sdk-auth > my.azureauth
```

### Set the auth file path to an environment variable

Follow one of the examples below depending on your operating system to create the environment variable. If using Windows close your opened IDE or shell and restart it to be able to read the environment variable.

Linux

```bash
export AZURE_AUTH_LOCATION="<YourAuthFilePath>"
```

Windows

```cmd
setx AZURE_AUTH_LOCATION "<YourAuthFilePath>"
```

## Run the application

First, clone the repository on your machine:

```bash
git clone https://github.com/Azure-Samples/service-bus-dotnet-manage-publish-subscribe-with-basic-features.git
```

Then, switch to the appropriate folder:
```bash
cd service-bus-dotnet-manage-publish-subscribe-with-basic-features
```

Finally, run the application with the `dotnet run` command.

```console
dotnet run
```

## Azure Service Bus basic scenario sample

 *  Create namespace.
 *  Create a topic.
 *  Update topic with new size and a new ServiceBus subscription.
 *  Create another ServiceBus subscription in the topic.
 *  List topic
 *  List ServiceBus subscriptions
 *  Get default authorization rule.
 *  Regenerate the keys in the authorization rule.
 *  Send a message to topic using Data plan sdk for Service Bus.
 *  Delete one ServiceBus subscription as part of update of topic.
 *  Delete another ServiceBus subscription.
 *  Delete topic
 *  Delete namespace

## More information

[Azure Management Libraries for C#][Azure .Net Developer Center]

---

This project has adopted the [Microsoft Open Source Code of Conduct]. For more information see the [Code of Conduct FAQ] or contact [opencode@microsoft.com] with any additional questions or comments.

<!-- LINKS -->
[free account]: https://azure.microsoft.com/free/?WT.mc_id=A261C142F
[Azure Management Libraries for C#]: https://github.com/Azure/azure-sdk-for-net/tree/Fluent
[Azure .Net Developer Center]: https://azure.microsoft.com/en-us/develop/net
[Microsoft Open Source Code of Conduct]: https://opensource.microsoft.com/codeofconduct
[opencode@microsoft.com]: mailto:opencode@microsoft.com
[Code of Conduct FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[Azure CLI 2.0]: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
