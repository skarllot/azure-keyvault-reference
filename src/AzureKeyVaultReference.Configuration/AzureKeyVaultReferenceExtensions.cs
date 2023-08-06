using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;
using Raiqub.AzureKeyVaultReference.Configuration.WrapProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Extension methods for adding support for Azure Key Vault references.
/// </summary>
public static class AzureKeyVaultReferenceExtensions
{
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    public static void AddAzureKeyVaultReferenceResolver(
        this ConfigurationManager configurationManager,
        AzureKeyVaultReferenceOptions? options = null)
    {
        options ??= new AzureKeyVaultReferenceOptions();
        ((IConfigurationBuilder)configurationManager).Add(
            new AzureKeyVaultReferenceRecursiveSource(options, new KeyVaultReferencesManager()));
    }

    public static void ConfigureAppConfigurationWithKeyVaultReferenceResolver(
        this IHostBuilder hostBuilder,
        Action<IConfigurationBuilder> configureDelegate,
        AzureKeyVaultReferenceOptions? options = null)
    {
        hostBuilder.Properties[typeof(InternalProperty)] = new InternalProperty(
            configureDelegate,
            options ?? new AzureKeyVaultReferenceOptions());

        hostBuilder.ConfigureAppConfiguration(Configure);
    }

    private static void Configure(HostBuilderContext context, IConfigurationBuilder builder)
    {
        var property = (InternalProperty)context.Properties[typeof(InternalProperty)];
        var configurationManager = new ConfigurationManager();
        property.ConfigureDelegate(configurationManager);

        builder.Add(
            new AzureKeyVaultReferenceWrapSource(
                configurationManager,
                property.Options,
                new KeyVaultReferencesManager(property.Options.Credential)));
    }

    private sealed record InternalProperty(
        Action<IConfigurationBuilder> ConfigureDelegate,
        AzureKeyVaultReferenceOptions Options);
}
