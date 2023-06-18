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
    Response<KeyVaultSecret> GetSecretValue(KeyVaultSecretReference secretReference);
}
