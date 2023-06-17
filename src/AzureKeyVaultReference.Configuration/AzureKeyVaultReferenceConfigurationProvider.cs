using Azure.Core;
using Colors.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Raiqub.AzureKeyVaultReference.Configuration;

internal sealed class AzureKeyVaultReferenceConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly IConfigurationRoot _configuration;
    private readonly KeyVaultReferencesManager _keyVault;
    private readonly ConfigurationReloadToken _reloadToken = new();
    private readonly object _gate = new();
    private bool _isLocked;

    public AzureKeyVaultReferenceConfigurationProvider(IConfigurationRoot configuration, TokenCredential? credential = null)
    {
        _configuration = configuration;
        _keyVault = new KeyVaultReferencesManager(credential);
        _keyVault.InvalidKeyVaultReference += OnInvalidKeyVaultReference;
    }

    public void Dispose()
    {
        _keyVault.InvalidKeyVaultReference -= OnInvalidKeyVaultReference;
        _keyVault.Dispose();
    }

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
                value = _configuration[key];
                value = _keyVault.GetSecretValue(key, value)?.Value ?? value;
                return true;
            }
            finally
            {
                _isLocked = false;
            }
        }
    }

    public void Set(string key, string? value)
    {
        throw new NotSupportedException();
    }

    public IChangeToken GetReloadToken()
    {
        return _reloadToken;
    }

    public void Load()
    {
        // Secrets are resolved on-demand
    }

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
                IEnumerable<IConfigurationSection> sections = parentPath is null
                    ? _configuration.GetChildren()
                    : _configuration.GetSection(parentPath).GetChildren();

                return sections
                    .Select(s => s.Key)
                    .Concat(earlierKeys)
                    .OrderBy(static x => x, ConfigurationKeyComparer.Instance);
            }
            finally
            {
                _isLocked = false;
            }
        }
    }

    private static void OnInvalidKeyVaultReference(string key)
    {
        ColoredConsole.WriteLine(
            StringStaticMethods.DarkYellow($"Unable to parse the Key Vault reference for setting: {key}"));
    }
}
