using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using Raiqub.AzureKeyVaultReference.Configuration.Factories;

namespace Raiqub.AzureKeyVaultReference.Configuration.BaseProvider;

public partial class AzureKeyVaultReferenceBaseProvider : IDisposable
{
    private readonly TimeSpan _cacheRetentionTime;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;
    private readonly IMemoryCache _memoryCache;
    private MemoryConfigurationProvider? _addedValues;

    protected AzureKeyVaultReferenceBaseProvider(
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _cacheRetentionTime = options.CacheRetentionTime;
        _loggerFactory = ConsoleLoggerFactory.Create(options);
        _logger = _loggerFactory.CreateLogger(GetType());
        _keyVaultReferencesManager = keyVaultReferencesManager;
        _memoryCache = MemoryCacheFactory.Create(options, _loggerFactory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache.Dispose();
            _keyVaultReferencesManager.Dispose();
            _loggerFactory.Dispose();
        }
    }

    protected IEnumerable<string> GetMemoryChildKeys(string? parentPath)
    {
        return _addedValues is not null
            ? _addedValues.GetChildKeys(Enumerable.Empty<string>(), parentPath)
            : Enumerable.Empty<string>();
    }

    protected void SetMemoryValue(string key, string? value)
    {
        _addedValues ??= new MemoryConfigurationProvider(new MemoryConfigurationSource());
        _addedValues.Set(key, value);
        _memoryCache.Remove(key);
    }

    protected bool TryGetInternal(
        string key,
        IConfiguration configuration,
        [NotNullWhen(true)] out string? value)
    {
        if (_memoryCache.TryGetValue(key, out object cacheValue))
        {
            value = (string)cacheValue;
            return true;
        }

        string? originalValue = _addedValues?.TryGet(key, out string? addedValue) is true
            ? addedValue
            : configuration[key];

        value = TryGetFromKeyVault(key, originalValue);
        return value is not null;
    }

    protected IEnumerable<string> GetChildKeysInternal(
        IConfiguration configuration,
        IEnumerable<string> earlierKeys,
        string? parentPath)
    {
        IEnumerable<IConfigurationSection> sections = parentPath is null
            ? configuration.GetChildren()
            : configuration.GetSection(parentPath).GetChildren();

        return sections
            .Select(s => s.Key)
            .Concat(GetMemoryChildKeys(parentPath))
            .Concat(earlierKeys)
            .OrderBy(static x => x, ConfigurationKeyComparer.Instance);
    }

    private void SetCache(string key, string value)
    {
        using var entry = _memoryCache.CreateEntry(key);
        entry.Size = 1L;
        entry.AbsoluteExpirationRelativeToNow = _cacheRetentionTime;
        entry.Value = value;
    }

    private string? TryGetFromKeyVault(string key, string? originalValue)
    {
        // Determine if the secret value is attempting to use a key vault reference
        if (KeyVaultReferenceParser.IsKeyVaultReference(originalValue) is false)
        {
            return originalValue;
        }

        // If we detect that a key vault reference was attempted, but did not match any of
        // the supported formats, we write a warning to the console.
        if (KeyVaultSecretReference.TryParse(originalValue, out var result) is false)
        {
            LogParseError(key);
            return originalValue;
        }

        Response<KeyVaultSecret> response;
        try
        {
            response = _keyVaultReferencesManager.GetSecretValue(result);
        }
        catch (RequestFailedException e)
        {
            LogGetError(e, result.ToString());
            return originalValue;
        }

        SetCache(key, response.Value.Value);
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
    private partial void LogGetError(RequestFailedException exception, string uri);
}
