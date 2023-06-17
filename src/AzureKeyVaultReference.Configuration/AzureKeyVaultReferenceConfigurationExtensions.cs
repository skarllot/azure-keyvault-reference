using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration;

/// <summary>
/// Extension methods for adding support for Azure Key Vault references.
/// </summary>
public static class AzureKeyVaultReferenceConfigurationExtensions
{
    /// <summary>
    /// Configure <see cref="ConfigurationManager"/> to support resolving Azure Key Vault references.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to configure.</param>
    /// <param name="credential">The credential to to use for authentication.</param>
    public static void AddAzureKeyVaultReferenceResolver(
        this ConfigurationManager configurationManager,
        TokenCredential? credential = null)
    {
        ((IConfigurationBuilder)configurationManager).Add(new AzureKeyVaultReferenceConfigurationSource(credential));
    }
}
