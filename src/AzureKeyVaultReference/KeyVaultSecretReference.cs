using System.Diagnostics.CodeAnalysis;

namespace Raiqub.AzureKeyVaultReference;

/// <summary>Represents a reference to a secret of Azure Key Vault.</summary>
/// <param name="VaultUri">The URI of Key Vault resource.</param>
/// <param name="Name">The name of the secret.</param>
/// <param name="Version">The version of the secret to use.</param>
/// <seealso href="https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references?tabs=azure-cli#reference-syntax"/>
public record KeyVaultSecretReference(Uri VaultUri, string Name, string? Version)
{
    /// <summary>
    /// Converts the string representation of a Key Vault reference to a <see cref="KeyVaultSecretReference"/> instance.
    /// A return value indicates whether the conversion succeeded.</summary>
    /// <param name="value">A string containing a Key Vault reference.</param>
    /// <param name="result">
    /// When this method returns, contains an instance of <see cref="KeyVaultSecretReference"/> equivalent to the value
    /// contained in <paramref name="value" />, if the conversion succeeded, or zero if the conversion failed.
    /// The conversion fails if the <paramref name="value" /> parameter is <see langword="null" /> or
    /// is not in a valid format. This parameter is passed uninitialized; any value originally supplied in
    /// <paramref name="result" /> will be overwritten.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.
    /// </returns>
    public static bool TryParse(string? value, [NotNullWhen(true)] out KeyVaultSecretReference? result) =>
        KeyVaultReferenceParser.TryParse(value, out result);


    /// <summary>Converts the string representation of a Key Vault reference to a <see cref="KeyVaultSecretReference"/> instance.</summary>
    /// <param name="value">A string containing a Key Vault reference.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    /// <exception cref="FormatException"><paramref name="value" /> is not in the correct format.</exception>
    /// <returns>An instance of <see cref="KeyVaultSecretReference"/> equivalent to the value contained in <paramref name="value" />.</returns>
    public static KeyVaultSecretReference Parse(string value)
    {
#if NET6_0
        ArgumentNullException.ThrowIfNull(value);
#else
        if (value is null)
            throw new ArgumentNullException(nameof(value));
#endif

        return KeyVaultReferenceParser.TryParse(value, out var result)
            ? result
            : throw new FormatException();
    }
}
