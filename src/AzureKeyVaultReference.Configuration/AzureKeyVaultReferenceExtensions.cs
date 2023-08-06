using Microsoft;
using Microsoft.Extensions.Configuration;
using Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;
using Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Extension methods for adding support for Azure Key Vault references.
/// </summary>
public static class AzureKeyVaultReferenceExtensions
{
    /// <summary>Adds the Azure Key Vault reference proxy provider to <paramref name="builder"/>.</summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureDelegate">
    /// The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used to construct the
    /// <see cref="IConfiguration"/> to be proxied by the Azure Key Vault reference provider.
    /// </param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddAzureKeyVaultReference(
        this IConfigurationBuilder builder,
        Action<IConfigurationBuilder> configureDelegate,
        AzureKeyVaultReferenceOptions? options = null)
    {
        options ??= new AzureKeyVaultReferenceOptions();

        return AddAzureKeyVaultReference(
            builder,
            configureDelegate,
            new KeyVaultReferencesManager(options.Credential),
            options);
    }

    public static IConfigurationBuilder AddAzureKeyVaultReference(
        this IConfigurationBuilder builder,
        Action<IConfigurationBuilder> configureDelegate,
        IKeyVaultReferencesManager keyVaultReferencesManager,
        AzureKeyVaultReferenceOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureDelegate);
        ArgumentNullException.ThrowIfNull(keyVaultReferencesManager);

        var configurationManager = new ConfigurationManager();
        configureDelegate(configurationManager);
#else
        Requires.NotNull(builder);
        Requires.NotNull(configureDelegate);
        Requires.NotNull(keyVaultReferencesManager);

        var internalBuilder = new ConfigurationBuilder();
        configureDelegate(internalBuilder);
        var configurationManager = internalBuilder.Build();
#endif

        return builder.Add(
            new AzureKeyVaultReferenceProxySource(
                configurationManager,
                options ?? new AzureKeyVaultReferenceOptions(),
                keyVaultReferencesManager));
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    [Obsolete("Use AddAzureKeyVaultReference extension method for IConfigurationBuilder")]
    public static void AddAzureKeyVaultReferenceResolver(
        this ConfigurationManager configurationManager,
        AzureKeyVaultReferenceOptions? options = null)
    {
        options ??= new AzureKeyVaultReferenceOptions();
        ((IConfigurationBuilder)configurationManager).Add(
            new AzureKeyVaultReferenceRecursiveSource(options, new KeyVaultReferencesManager(options.Credential)));
    }
#endif
}
