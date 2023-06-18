using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging.Console;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Options class used by the <see cref="AzureKeyVaultReferenceExtensions"/>.
/// </summary>
public class AzureKeyVaultReferenceOptions
{
    /// <summary>
    /// Gets or sets the credential to to use for authentication (default: <see cref="DefaultAzureCredential"/>).
    /// </summary>
    public TokenCredential Credential { get; set; } = new DefaultAzureCredential();

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
}
