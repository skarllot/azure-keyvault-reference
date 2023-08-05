using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Raiqub.AzureKeyVaultReference.Configuration.Factories;

namespace Raiqub.AzureKeyVaultReference.Configuration;

public sealed partial class AzureKeyVaultReferenceProvider : IConfigurationProvider, IDisposable
{
    private readonly IConfigurationRoot _configuration;
    private readonly TimeSpan _cacheRetentionTime;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AzureKeyVaultReferenceProvider> _logger;
    private readonly KeyVaultReferencesManager _keyVault;
    private readonly IMemoryCache _memoryCache;
    private readonly ConfigurationReloadToken _reloadToken = new();
    private readonly object _gate = new();
    private bool _isLocked;

    public AzureKeyVaultReferenceProvider(IConfigurationRoot configuration, AzureKeyVaultReferenceOptions options)
    {
        _configuration = configuration;
        _cacheRetentionTime = options.CacheRetentionTime;
        _loggerFactory = ConsoleLoggerFactory.Create(options);
        _logger = _loggerFactory.CreateLogger<AzureKeyVaultReferenceProvider>();
        _keyVault = new KeyVaultReferencesManager(options.Credential);
        _memoryCache = MemoryCacheFactory.Create(options, _loggerFactory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _memoryCache.Dispose();
        _keyVault.Dispose();
        _loggerFactory.Dispose();
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
                string originalValue = _configuration[key];

                // Determine if the secret value is attempting to use a key vault reference
                if (KeyVaultReferenceParser.IsKeyVaultReference(originalValue))
                {
                    value = TryGetFromKeyVault(key, originalValue) ?? originalValue;
                }
                else
                {
                    value = originalValue;
                }

                return true;
            }
            finally
            {
                _isLocked = false;
            }
        }
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Does not support setting configuration values.</exception>
    public void Set(string key, string? value)
    {
        throw new NotSupportedException();
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

    private string? TryGetFromKeyVault(string key, string originalValue)
    {
        if (_memoryCache.TryGetValue(key, out object cacheValue))
        {
            return (string)cacheValue;
        }

        // If we detect that a key vault reference was attempted, but did not match any of
        // the supported formats, we write a warning to the console.
        if (KeyVaultSecretReference.TryParse(originalValue, out var result) is false)
        {
            LogParseError(key);
            return null;
        }

        Response<KeyVaultSecret> response = _keyVault.GetSecretValue(result);
        if (response.HasValue is false)
        {
            LogGetError(result.ToString());
            return null;
        }

        using var entry = _memoryCache.CreateEntry(key);
        entry.Size = 1L;
        entry.AbsoluteExpirationRelativeToNow = _cacheRetentionTime;
        entry.Value = response.Value.Value;

        return response.Value.Value;
    }

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Warning,
        Message = "Unable to parse the Key Vault reference for setting: {Key}")]
    private partial void LogParseError(string key);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unable to get secret from the Key Vault: {Uri}")]
    private partial void LogGetError(string uri);
}
