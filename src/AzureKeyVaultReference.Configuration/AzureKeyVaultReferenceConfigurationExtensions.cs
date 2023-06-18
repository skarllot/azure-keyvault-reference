using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Extension methods for adding support for Azure Key Vault references.
/// </summary>
public static class AzureKeyVaultReferenceConfigurationExtensions
{
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="credential">The credential to to use for authentication.</param>
    /// <param name="configureLog">The <see cref="ILoggingBuilder"/> configuration delegate.</param>
    /// <param name="configureCache">Configure the provided <see cref="MemoryCacheOptions"/>.</param>
    public static void AddAzureKeyVaultReferenceResolver(
        this ConfigurationManager configurationManager,
        TokenCredential? credential = null,
        Action<ILoggingBuilder>? configureLog = null,
        Action<MemoryCacheOptions>? configureCache = null)
    {
        configureLog ??= static builder => builder.AddConsole();
        configureCache ??= static _ => { };

        var services = new ServiceCollection()
            .AddSingleton(credential ?? new DefaultAzureCredential())
            .AddTransient<KeyVaultReferencesManager>()
            .AddLogging(configureLog)
            .AddMemoryCache(configureCache);

        ((IConfigurationBuilder)configurationManager).Add(
            new AzureKeyVaultReferenceConfigurationSource(services));
    }
}
