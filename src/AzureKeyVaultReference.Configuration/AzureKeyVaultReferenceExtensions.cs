using Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> or <paramref name="configureDelegate"/> is <see langword="null" />.</exception>
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

    /// <summary>Adds the Azure Key Vault reference proxy provider to <paramref name="builder"/>.</summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureDelegate">
    /// The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used to construct the
    /// <see cref="IConfiguration"/> to be proxied by the Azure Key Vault reference provider.
    /// </param>
    /// <param name="keyVaultReferencesManager">Manager for retrieving Key Vault secrets values.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" />, <paramref name="configureDelegate"/> or <paramref name="keyVaultReferencesManager"/> is <see langword="null" />.</exception>
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
#else
        Requires.NotNull(builder);
        Requires.NotNull(configureDelegate);
        Requires.NotNull(keyVaultReferencesManager);

#endif

        var internalBuilder = new ConfigurationBuilder();
        configureDelegate(internalBuilder);

        return builder.Add(
            new AzureKeyVaultReferenceProxySource(
                internalBuilder.Build(),
                options ?? new AzureKeyVaultReferenceOptions(),
                keyVaultReferencesManager));
    }

    /// <summary>
    /// Configures a Azure Key Vault reference provider as proxy of existing <see cref="IHostBuilder"/> configuration
    /// sources. To avoid the proxy being overwritten, ensure this is called after all configuration sources are added.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostBuilder" /> is <see langword="null" />.</exception>
    public static IHostBuilder ConfigureAzureKeyVaultReference(
        this IHostBuilder hostBuilder,
        AzureKeyVaultReferenceOptions? options = null)
    {
        options ??= new AzureKeyVaultReferenceOptions();
        return ConfigureAzureKeyVaultReference(hostBuilder, new KeyVaultReferencesManager(options.Credential), options);
    }

    /// <summary>
    /// Configures a Azure Key Vault reference provider as proxy of existing <see cref="IHostBuilder"/> configuration
    /// sources. To avoid the proxy being overwritten, ensure this is called after all configuration sources are added.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="keyVaultReferencesManager">Manager for retrieving Key Vault secrets values.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostBuilder" /> or <paramref name="keyVaultReferencesManager"/> is <see langword="null" />.</exception>
    public static IHostBuilder ConfigureAzureKeyVaultReference(
        this IHostBuilder hostBuilder,
        IKeyVaultReferencesManager keyVaultReferencesManager,
        AzureKeyVaultReferenceOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(keyVaultReferencesManager);
#else
        Requires.NotNull(hostBuilder);
        Requires.NotNull(keyVaultReferencesManager);
#endif

        return hostBuilder.ConfigureAppConfiguration(
            (_, builder) => builder.Add(
                new AzureKeyVaultReferenceProxySource(
                    builder.MoveSourcesToNewConfiguration(),
                    options ?? new AzureKeyVaultReferenceOptions(),
                    keyVaultReferencesManager)));
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    [Obsolete("Use ConfigureAzureKeyVaultReference extension method for IHostBuilder")]
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
