using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Extension methods for adding support for Azure Key Vault references.
/// </summary>
public static class AzureKeyVaultReferenceExtensions
{
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="options">The <see cref="AzureKeyVaultReferenceOptions"/> to use.</param>
    public static void AddAzureKeyVaultReferenceResolver(
        this ConfigurationManager configurationManager,
        AzureKeyVaultReferenceOptions? options = null)
    {
        options ??= new AzureKeyVaultReferenceOptions();
        ((IConfigurationBuilder)configurationManager).Add(new AzureKeyVaultReferenceSource(options));
    }
}
