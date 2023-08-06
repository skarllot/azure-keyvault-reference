using Azure;
using Azure.Security.KeyVault.Secrets;

namespace Raiqub.AzureKeyVaultReference;

/// <summary>Manager for retrieving Key Vault secrets values.</summary>
public interface IKeyVaultReferencesManager : IDisposable
{
    /// <summary>
    /// Gets the Key Vault secret value from specified <see cref="KeyVaultSecretReference"/>.
    /// </summary>
    /// <param name="secretReference">The secret reference to retrieve the value.</param>
    /// <returns>The result of Key Vault secret value retrieval.</returns>
    /// <exception cref="RequestFailedException">The server returned an error. See <see cref="Exception.Message"/> for details returned from the server.</exception>
    Response<KeyVaultSecret> GetSecretValue(KeyVaultSecretReference secretReference);
}
