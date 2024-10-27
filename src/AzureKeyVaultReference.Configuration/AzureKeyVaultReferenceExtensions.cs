using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;

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
    /// <param name="optionsAction">An optional action to configure the <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> or <paramref name="configureDelegate"/> is <see langword="null" />.</exception>
    public static IConfigurationBuilder AddAzureKeyVaultReference(
        this IConfigurationBuilder builder,
        Action<IConfigurationBuilder> configureDelegate,
        Action<AzureKeyVaultReferenceOptions>? optionsAction = null)
    {
        return AddAzureKeyVaultReference(builder, configureDelegate, null, optionsAction);
    }

    /// <summary>Adds the Azure Key Vault reference proxy provider to <paramref name="builder"/>.</summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureDelegate">
    /// The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used to construct the
    /// <see cref="IConfiguration"/> to be proxied by the Azure Key Vault reference provider.
    /// </param>
    /// <param name="keyVaultReferencesManager">Manager for retrieving Key Vault secrets values.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" />, <paramref name="configureDelegate"/> or <paramref name="keyVaultReferencesManager"/> is <see langword="null" />.</exception>
    public static IConfigurationBuilder AddAzureKeyVaultReference(
        this IConfigurationBuilder builder,
        Action<IConfigurationBuilder> configureDelegate,
        IKeyVaultReferencesManager? keyVaultReferencesManager,
        Action<AzureKeyVaultReferenceOptions>? optionsAction = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureDelegate);
#else
        Polyfill.Requires.NotNull(builder);
        Polyfill.Requires.NotNull(configureDelegate);
#endif

        var options = new AzureKeyVaultReferenceOptions();
        optionsAction?.Invoke(options);

        var internalBuilder = new ConfigurationBuilder();
        configureDelegate(internalBuilder);

        return builder.Add(
            new AzureKeyVaultReferenceProxySource(
                internalBuilder.Build(),
                options,
                keyVaultReferencesManager ?? new KeyVaultReferencesManager(options.Credential)));
    }

    /// <summary>
    /// Configures a Azure Key Vault reference provider as proxy of existing <see cref="IHostBuilder"/> configuration
    /// sources. To avoid the proxy being overwritten, ensure this is called after all configuration sources are added.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostBuilder" /> is <see langword="null" />.</exception>
    public static IHostBuilder ConfigureAzureKeyVaultReference(
        this IHostBuilder hostBuilder,
        Action<AzureKeyVaultReferenceOptions>? optionsAction = null)
    {
        return ConfigureAzureKeyVaultReference(hostBuilder, null, optionsAction);
    }

    /// <summary>
    /// Configures a Azure Key Vault reference provider as proxy of existing <see cref="IHostBuilder"/> configuration
    /// sources. To avoid the proxy being overwritten, ensure this is called after all configuration sources are added.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="keyVaultReferencesManager">Manager for retrieving Key Vault secrets values.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostBuilder" /> or <paramref name="keyVaultReferencesManager"/> is <see langword="null" />.</exception>
    public static IHostBuilder ConfigureAzureKeyVaultReference(
        this IHostBuilder hostBuilder,
        IKeyVaultReferencesManager? keyVaultReferencesManager,
        Action<AzureKeyVaultReferenceOptions>? optionsAction = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        Polyfill.Requires.NotNull(hostBuilder);
#endif

        return hostBuilder.ConfigureAppConfiguration(
            (_, builder) =>
            {
                var options = new AzureKeyVaultReferenceOptions();
                optionsAction?.Invoke(options);

                builder.Add(
                    new AzureKeyVaultReferenceProxySource(
                        builder.MoveSourcesToNewConfiguration(),
                        options,
                        keyVaultReferencesManager ?? new KeyVaultReferencesManager(options.Credential)));
            });
    }
}
