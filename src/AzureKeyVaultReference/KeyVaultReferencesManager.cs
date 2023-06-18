using System.Collections.Concurrent;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Raiqub.AzureKeyVaultReference;

public sealed partial class KeyVaultReferencesManager : IDisposable
{
    private readonly TokenCredential _credential;
    private readonly ILogger<KeyVaultReferencesManager> _logger;
    private readonly ConcurrentDictionary<string, SecretClient> _clients = new();

    public KeyVaultReferencesManager(
        TokenCredential? credential = null,
        ILogger<KeyVaultReferencesManager>? logger = null)
    {
        _credential = credential ?? new DefaultAzureCredential();
        _logger = logger ?? NullLogger<KeyVaultReferencesManager>.Instance;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _clients.Clear();
    }

    public KeyVaultSecret? GetSecretValue(string key, string? value)
    {
        if (value == null)
        {
            return null;
        }

        var result = ParseSecret(key, value);
        if (result == null)
        {
            return null;
        }

        var client = GetSecretClient(result.VaultUri);
        var secret = client.GetSecret(result.Name, result.Version);
        if (secret.HasValue is false)
        {
            LogGetError(result.ToString());
            return null;
        }

        return secret.Value;

    }

    private KeyVaultSecretReference? ParseSecret(string key, string? value)
    {
        // Determine if the secret value is attempting to use a key vault reference
        if (KeyVaultReferenceParser.IsKeyVaultReference(value) is false)
        {
            return null;
        }

        bool isParsed = KeyVaultSecretReference.TryParse(value, out var result);

        // If we detect that a key vault reference was attempted, but did not match any of
        // the supported formats, we write a warning to the console.
        if (!isParsed)
        {
            LogParseError(key);
        }

        return result;
    }

    private SecretClient GetSecretClient(Uri vaultUri)
    {
#if NET6_0
        return _clients.GetOrAdd(
            vaultUri.ToString(),
            static (_, arg) => new SecretClient(arg.vaultUri, arg.credential),
            (vaultUri, credential: _credential));
#else
        return _clients.GetOrAdd(
            vaultUri.ToString(),
            _ => new SecretClient(vaultUri, _credential));
#endif
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
