using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;

/// <summary>
/// Represents a configuration source that proxies an <see cref="IConfigurationRoot"/>
/// to resolve Azure Key Vault references.
/// </summary>
public class AzureKeyVaultReferenceProxySource : IConfigurationSource
{
    private readonly IConfigurationRoot _internalConfiguration;
    private readonly AzureKeyVaultReferenceOptions _options;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultReferenceProxySource"/> class.
    /// </summary>
    /// <param name="internalConfiguration">The configuration to be proxied.</param>
    /// <param name="options">The options for configuring Azure Key Vault references.</param>
    /// <param name="keyVaultReferencesManager">The manager responsible for resolving Azure Key Vault references.</param>
    public AzureKeyVaultReferenceProxySource(
        IConfigurationRoot internalConfiguration,
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _internalConfiguration = internalConfiguration;
        _options = options;
        _keyVaultReferencesManager = keyVaultReferencesManager;
    }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AzureKeyVaultReferenceProxyProvider(
            _internalConfiguration,
            _options,
            _keyVaultReferencesManager);
    }
}
