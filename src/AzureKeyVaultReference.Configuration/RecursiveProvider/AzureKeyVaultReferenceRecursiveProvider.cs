using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Raiqub.AzureKeyVaultReference.Configuration.BaseProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;

/// <summary>
/// Represents a configuration provider that recursively resolves Key Vault references in the configuration.
/// </summary>
public sealed class AzureKeyVaultReferenceRecursiveProvider
    : AzureKeyVaultReferenceBaseProvider, IConfigurationProvider
{
    private readonly IConfigurationRoot _configuration;
    private readonly object _gate = new();
    private readonly ConfigurationReloadToken _reloadToken = new();
    private bool _isLocked;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultReferenceRecursiveProvider"/> class with the
    /// specified configuration, options, and Key Vault references manager.
    /// </summary>
    /// <param name="configuration">The root configuration object.</param>
    /// <param name="options">The configuration options for the provider.</param>
    /// <param name="keyVaultReferencesManager">The manager responsible for handling Key Vault references.</param>
    public AzureKeyVaultReferenceRecursiveProvider(
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
        lock (_gate)
        {
            if (_isLocked)
            {
                value = null;
                return false;
            }

            try
            {
                _isLocked = true;
                return TryGetInternal(key, _configuration, out value);
            }
            finally
            {
                _isLocked = false;
            }
        }
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
        lock (_gate)
        {
            if (_isLocked)
            {
                return earlierKeys;
            }

            try
            {
                _isLocked = true;
                return GetChildKeysInternal(_configuration, earlierKeys, parentPath);
            }
            finally
            {
                _isLocked = false;
            }
        }
    }
}
