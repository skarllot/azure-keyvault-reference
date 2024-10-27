using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using Raiqub.AzureKeyVaultReference.Configuration.Factories;

namespace Raiqub.AzureKeyVaultReference.Configuration.BaseProvider;

/// <summary>
/// Base provider for handling Azure Key Vault references within a configuration.
/// </summary>
public partial class AzureKeyVaultReferenceBaseProvider : IDisposable
{
    private readonly TimeSpan _cacheRetentionTime;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;
    private readonly IMemoryCache _memoryCache;
    private readonly string? _defaultVaultNameOrUri;
    private MemoryConfigurationProvider? _addedValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultReferenceBaseProvider"/> class.
    /// </summary>
    /// <param name="options">Options for configuring the Azure Key Vault reference provider.</param>
    /// <param name="keyVaultReferencesManager">Manager for resolving Azure Key Vault references.</param>
    protected AzureKeyVaultReferenceBaseProvider(
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _cacheRetentionTime = options.CacheRetentionTime;
        _loggerFactory = ConsoleLoggerFactory.Create(options);
        _logger = _loggerFactory.CreateLogger(GetType());
        _keyVaultReferencesManager = keyVaultReferencesManager;
        _memoryCache = MemoryCacheFactory.Create(options, _loggerFactory);
        _defaultVaultNameOrUri = options.GetDefaultVaultNameOrUri();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by the <see cref="AzureKeyVaultReferenceBaseProvider"/> object.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache.Dispose();
            _keyVaultReferencesManager.Dispose();
            _loggerFactory.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the immediate descendant configuration keys for a given parent path.
    /// </summary>
    /// <param name="parentPath">The parent path.</param>
    /// <returns>The child keys.</returns>
    protected IEnumerable<string> GetMemoryChildKeys(string? parentPath)
    {
        return _addedValues is not null
            ? _addedValues.GetChildKeys(Enumerable.Empty<string>(), parentPath)
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Sets a configuration value for the specified key.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    protected void SetMemoryValue(string key, string? value)
    {
        _addedValues ??= new MemoryConfigurationProvider(new MemoryConfigurationSource());
        _addedValues.Set(key, value);
        _memoryCache.Remove(key);
    }

    /// <summary>
    /// Attempts to get the value for a specified key.
    /// </summary>
    /// <param name="key">The key to retrieve.</param>
    /// <param name="configuration">The configuration to query.</param>
    /// <param name="value">The value corresponding to the key, if found.</param>
    /// <returns>True if the key was found and its value retrieved; otherwise, false.</returns>
    protected bool TryGetInternal(
        string key,
        IConfiguration configuration,
        out string? value)
    {
        if (_memoryCache.TryGetValue(key, out object? cacheValue))
        {
            value = (string?)cacheValue;
            return true;
        }

        string? originalValue = _addedValues?.TryGet(key, out string? addedValue) is true
            ? addedValue
            : configuration[key];

        value = TryGetFromKeyVault(key, originalValue);
        return value is not null;
    }

    /// <summary>
    /// Retrieves the immediate descendant configuration keys for a given parent path.
    /// </summary>
    /// <param name="configuration">The configuration to query.</param>
    /// <param name="earlierKeys">The child keys returned by the preceding providers for the same parent path.</param>
    /// <param name="parentPath">The parent path.</param>
    /// <returns>The child keys.</returns>
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

    private void SetCache(string key, string? value)
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
        if (KeyVaultSecretReference.TryParse(originalValue, _defaultVaultNameOrUri, out var result) is false)
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

#if NET6_0_OR_GREATER
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
#else
    private static readonly Action<ILogger, string, Exception?> s_logParseErrorCallback = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(0, nameof(LogParseError)),
        "Unable to parse the Key Vault reference for setting: {Key}");

    private static readonly Action<ILogger, string, Exception?> s_logGetErrorCallback = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(0, nameof(LogParseError)),
        "Unable to get secret from the Key Vault: {Uri}");

    private void LogParseError(string key) =>
        s_logParseErrorCallback(_logger, key, null);

    private void LogGetError(RequestFailedException exception, string uri) =>
        s_logGetErrorCallback(_logger, uri, exception);
#endif
}
