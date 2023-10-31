# Azure Key Vault Reference

[![Build status](https://github.com/skarllot/azure-keyvault-reference/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/skarllot/azure-keyvault-reference/actions)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/skarllot/azure-keyvault-reference/badge)](https://securityscorecards.dev/viewer/?uri=github.com/skarllot/azure-keyvault-reference)
[![Code coverage](https://codecov.io/gh/skarllot/azure-keyvault-reference/branch/main/graph/badge.svg)](https://codecov.io/gh/skarllot/azure-keyvault-reference)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fskarllot%2Fazure-keyvault-reference%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/skarllot/azure-keyvault-reference/main)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://raw.githubusercontent.com/skarllot/Expressions/master/LICENSE)

_The Raiqub Azure Key Vault Reference NuGet packages simplifies the integration of Azure Key Vault with your .NET applications by providing support for Azure Key Vault references in the `IConfiguration` system._

[🏃 Quickstart](#quickstart) &nbsp; | &nbsp; [📗 Guide](#guide) &nbsp; | &nbsp; [📦 NuGet](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference.Configuration)

<hr />

## Features
* Seamless integration of Azure Key Vault references with `IConfiguration`
* Easy retrieval of secrets from configuration using Azure Key Vault references
* Support for parsing Azure Key Vault references from strings

## NuGet Packages
* [![NuGet](https://buildstats.info/nuget/Raiqub.AzureKeyVaultReference)](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference/) **Raiqub.AzureKeyVaultReference**: provides support for parsing Azure Key Vault references
* [![NuGet](https://buildstats.info/nuget/Raiqub.AzureKeyVaultReference.Configuration)](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference.Configuration/) **Raiqub.AzureKeyVaultReference.Configuration**: provides support for integrating Azure Key Vault references with `IConfiguration`

## Prerequisites
Before you begin, you'll need the following:

* .NET Standard 2.0 or .NET Core 6.0 installed on your machine
* An IDE such as Visual Studio, Visual Studio Code, or JetBrains Rider

## Quickstart
To use the library, you can install the desired NuGet package(s) in your Web project and add the configuration provider. Here's an example of how to add the configuration provider:

```csharp
var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAzureKeyVaultReference();
```

or using WebApplication

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAzureKeyVaultReference();
```

## Guide
To use the Azure Key Vault Configuration Provider, follow these steps:

1. **Set up Azure Key Vault**: Ensure you have an Azure Key Vault instance created and the necessary permissions to access it.
2. **Install and configure the package**: Install the NuGet package and add the necessary configuration to your application.
3. **Configure Azure Key Vault references**: In your **\`appsettings.json\`** file or any other configuration source, add Azure Key Vault references using the **\`@Microsoft.KeyVault\`** syntax. For example:

```json
{
  "MySecret": "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/MySecret)",
  "OtherSecret": "@Microsoft.KeyVault(VaultName=your-keyvault;SecretName=OtherSecret)"
}
```

4. **Retrieve configuration values**: Access the configuration values as usual using the **\`IConfiguration\`** interface. The Azure Key Vault Configuration Provider will automatically fetch the secrets from Azure Key Vault and replace the references with the corresponding values.

```csharp
using System.IO;
using Microsoft.Extensions.Configuration;
using Raiqub.AzureKeyVaultReference.Configuration;

var configuration = new ConfigurationManager()
    .AddAzureKeyVaultReference(builder =>
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json"))
    .Build();

var mySecretValue = configuration["MySecret"];
```

### Parsing Azure Key Vault references
If you need to parse Azure Key Vault references from strings programmatically, you can use the **\`KeyVaultSecretReference\`** class provided by this package.

```csharp
using Raiqub.AzureKeyVaultReference;

var reference = "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/MySecret)";

var parsedReference = KeyVaultSecretReference.Parse(reference);
// ParsedReference.VaultUri: "https://your-keyvault.vault.azure.net"
// ParsedReference.Name: "MySecret"
// ParsedReference.Version: null
```

### Default Azure Key Vault
This library supports defining a default Key Vault to use when one is not defined on Azure Key Vault reference.

```csharp
var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAzureKeyVaultReference(
    options => options.GetDefaultVaultNameOrUri = () => Environment.GetEnvironmentVariable("KEYVAULTURI"));
```

or using WebApplication

```csharp
builder.Host.ConfigureAzureKeyVaultReference(
    options => options.GetDefaultVaultNameOrUri = () => Environment.GetEnvironmentVariable("KEYVAULTNAME"));
```

Doing so the Azure Key Vault reference do not need to specify the Key Vault Name

```json
{
  "MySecret": "@Microsoft.KeyVault(SecretName=MySecret)"
}
```

## Contributing

If something is not working for you or if you think that the source file
should change, feel free to create an issue or Pull Request.
I will be happy to discuss and potentially integrate your ideas!

## License

This library is licensed under the [MIT License](./LICENSE).
