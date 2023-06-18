using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Raiqub.AzureKeyVaultReference.Configuration;

internal sealed class AzureKeyVaultReferenceConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly IConfigurationRoot _configuration;
    private readonly ServiceProvider _serviceProvider;
    private readonly KeyVaultReferencesManager _keyVault;
    private readonly ConfigurationReloadToken _reloadToken = new();
    private readonly object _gate = new();
    private bool _isLocked;

    public AzureKeyVaultReferenceConfigurationProvider(
        IConfigurationRoot configuration,
        IServiceCollection serviceCollection)
    {
        _configuration = configuration;
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _keyVault = _serviceProvider.GetRequiredService<KeyVaultReferencesManager>();
    }

    public void Dispose()
    {
        _keyVault.Dispose();
        _serviceProvider.Dispose();
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
}
