using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Raiqub.AzureKeyVaultReference;

/// <summary>Manager for retrieving Key Vault secrets values with cached client for each Key Vault resource.</summary>
public sealed class KeyVaultReferencesManager : IKeyVaultReferencesManager
{
    private readonly TokenCredential _credential;
    private readonly ConcurrentDictionary<string, SecretClient> _clients = new();

    public KeyVaultReferencesManager(TokenCredential? credential = null)
    {
        _credential = credential ?? new DefaultAzureCredential();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _clients.Clear();
    }

    /// <inheritdoc />
    public Response<KeyVaultSecret> GetSecretValue(KeyVaultSecretReference secretReference)
    {
        var client = GetSecretClient(secretReference.VaultUri);
        try
        {
            return client.GetSecret(secretReference.Name, secretReference.Version);
        }
        catch (AggregateException e) when (e.InnerExceptions.All(static innerException => innerException is RequestFailedException))
        {
            ExceptionDispatchInfo.Capture((RequestFailedException)e.InnerExceptions[0]).Throw();
            throw;
        }
    }

    private SecretClient GetSecretClient(Uri vaultUri)
    {
#if NET6_0_OR_GREATER
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
