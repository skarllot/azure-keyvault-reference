using System.Collections.Concurrent;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Raiqub.AzureKeyVaultReference;

public sealed class KeyVaultReferencesManager : IDisposable
{
    private readonly ConcurrentDictionary<string, SecretClient> _clients = new();
    private readonly TokenCredential _credential;

    public KeyVaultReferencesManager(TokenCredential? credential = null)
    {
        _credential = credential ?? new DefaultAzureCredential();
    }

    public event Action<string>? InvalidKeyVaultReference;

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

        if (result != null)
        {
            var client = GetSecretClient(result.VaultUri);
            var secret = client.GetSecret(result.Name, result.Version);
            return secret.Value;
        }

        return null;
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
            InvalidKeyVaultReference?.Invoke(key);
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
}
