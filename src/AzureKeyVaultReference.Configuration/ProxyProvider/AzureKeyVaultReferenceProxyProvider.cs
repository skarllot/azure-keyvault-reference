using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Raiqub.AzureKeyVaultReference.Configuration.BaseProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;

/// <summary>
/// A configuration provider that proxies an <see cref="IConfigurationRoot"/> to resolve Azure Key Vault references.
/// </summary>
public sealed class AzureKeyVaultReferenceProxyProvider : AzureKeyVaultReferenceBaseProvider, IConfigurationProvider
{
    private readonly IConfigurationRoot _configuration;
    private readonly ConfigurationReloadToken _reloadToken = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultReferenceProxyProvider"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfigurationRoot"/> to proxy.</param>
    /// <param name="options">Options for configuring the Azure Key Vault reference provider.</param>
    /// <param name="keyVaultReferencesManager">Manager for resolving Azure Key Vault references.</param>
    public AzureKeyVaultReferenceProxyProvider(
        IConfigurationRoot configuration,
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
        : base(options, keyVaultReferencesManager)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public bool TryGet(string key, out string? value)
    {
        return TryGetInternal(key, _configuration, out value);
    }

    /// <inheritdoc />
    public void Set(string key, string? value)
    {
        SetMemoryValue(key, value);
    }

    /// <inheritdoc />
    public IChangeToken GetReloadToken()
    {
        return _reloadToken;
    }

    /// <inheritdoc />
    public void Load()
    {
        // Secrets are resolved on-demand
    }

    /// <inheritdoc />
    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return GetChildKeysInternal(_configuration, earlierKeys, parentPath);
    }
}
