# Azure Key Vault Reference

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/EngRajabi/Enum.Source.Generator/master/LICENSE) [![Nuget](https://img.shields.io/nuget/v/Raiqub.AzureKeyVaultReference.Configuration)](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference.Configuration) [![Nuget](https://img.shields.io/nuget/dt/Raiqub.AzureKeyVaultReference.Configuration?label=Nuget.org%20Downloads&style=flat-square&color=blue)](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference.Configuration)

_The Raiqub Azure Key Vault Reference NuGet packages simplifies the integration of Azure Key Vault with your .NET applications by providing support for Azure Key Vault references in the `IConfiguration` system._

[🏃 Quickstart](#quickstart) &nbsp; | &nbsp; [📗 Guide](#guide) &nbsp; | &nbsp; [📦 NuGet](https://www.nuget.org/packages/Raiqub.AzureKeyVaultReference.Configuration)

<hr />

## Features
* Seamless integration of Azure Key Vault references with `IConfiguration`
* Easy retrieval of secrets and configuration settings from Azure Key Vault
* Support for parsing Azure Key Vault references from strings

## NuGet Packages
* **Raiqub.AzureKeyVaultReference**: provides support for parsing Azure Key Vault references
* **Raiqub.AzureKeyVaultReference.Configuration**: provides support for integrating Azure Key Vault references with `IConfiguration`

## Prerequisites
Before you begin, you'll need the following:

* .NET Standard 2.0 or .NET Core 6.0 installed on your machine
* An IDE such as Visual Studio, Visual Studio Code, or JetBrains Rider

## Quickstart
To use the library, you can install the desired NuGet package(s) in your Web project and add the configuration provider. Here's an example of how to add the configuration provider:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ...

// Just before building WebApplication
builder.Configuration.AddAzureKeyVaultReferenceResolver();

var app = builder.Build();
```

## Guide
To use the Azure Key Vault Configuration Provider, follow these steps:

1. **Set up Azure Key Vault**: Ensure you have an Azure Key Vault instance created and the necessary permissions to access it.
2. **Install and configure the package**: Install the NuGet package and add the necessary configuration to your application.
3. **Configure Azure Key Vault references**: In your **\`appsettings.json\`** file or any other configuration source, add Azure Key Vault references using the **\`@Microsoft.KeyVault\`** syntax. For example:

```json
{
  "MySecret": "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/MySecret)"
}
```

4. **Retrieve configuration values**: Access the configuration values as usual using the **\`IConfiguration\`** interface. The Azure Key Vault Configuration Provider will automatically fetch the secrets from Azure Key Vault and replace the references with the corresponding values.

```csharp
using System.IO;
using Microsoft.Extensions.Configuration;
using Raiqub.AzureKeyVaultReference.Configuration;

var configuration = new ConfigurationManager()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVaultReferenceResolver() // It must be always the last provider
    .Build();

var mySecretValue = configuration["MySecret"];
```

5. **Parsing Azure Key Vault references**: If you need to parse Azure Key Vault references from strings programmatically, you can use the **\`KeyVaultSecretReference\`** class provided by this package.

```csharp
using Raiqub.AzureKeyVaultReference;

var reference = "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/MySecret)";

var parsedReference = KeyVaultSecretReference.Parse(reference);
// ParsedReference.VaultUri: "https://your-keyvault.vault.azure.net"
// ParsedReference.Name: "MySecret"
// ParsedReference.Version: null
```

## Contributing

If something is not working for you or if you think that the source file
should change, feel free to create an issue or Pull Request.
I will be happy to discuss and potentially integrate your ideas!

## License

This library is licensed under the [MIT License](./LICENSE).
