using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging.Console;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Options for configuring the Azure Key Vault reference provider.
/// </summary>
public class AzureKeyVaultReferenceOptions
{
    private TokenCredential? _credential;

    /// <summary>
    /// Gets or sets the credential to to use for authentication (default: <see cref="DefaultAzureCredential"/>).
    /// </summary>
    public TokenCredential Credential
    {
        get => _credential ??= new DefaultAzureCredential();
        set => _credential = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of cache entries (default: 100).
    /// </summary>
    public int CacheSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time interval to keep each cache entry of a secret value (default: 30 minutes).
    /// </summary>
    public TimeSpan CacheRetentionTime { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the options for log using console logger.
    /// </summary>
    public ConsoleLoggerOptions LoggerOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the getter of default Key Vault name or URI when it is not defined on Key Vault reference.
    /// </summary>
    public Func<string?> GetDefaultVaultNameOrUri { get; set; } = static () => null;

    internal void CopyFrom(AzureKeyVaultReferenceOptions? options)
    {
        if (options is null)
            return;

        _credential = options._credential;
        CacheSize = options.CacheSize;
        CacheRetentionTime = options.CacheRetentionTime;
        LoggerOptions = options.LoggerOptions;
        GetDefaultVaultNameOrUri = options.GetDefaultVaultNameOrUri;
    }
}
