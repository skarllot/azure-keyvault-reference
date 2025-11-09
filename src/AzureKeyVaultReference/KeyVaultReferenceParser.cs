using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Azure.Security.KeyVault.Secrets;

namespace Raiqub.AzureKeyVaultReference;

/// <summary>Supports parsing Azure Key Vault references.</summary>
public static class KeyVaultReferenceParser
{
    private const string VaultUriSuffix = "vault.azure.net";

    private static readonly Regex s_basicKeyVaultReferenceRegex = new(
        @"^@Microsoft\.KeyVault\((?<ReferenceString>.*)\)$",
        RegexOptions.Compiled);

    private static readonly Regex s_secretUriRegex = new("SecretUri=(?<Value>[^;]+)(;|$)", RegexOptions.Compiled);
    private static readonly Regex s_vaultNameRegex = new("VaultName=(?<Value>[^;]+)(;|$)", RegexOptions.Compiled);
    private static readonly Regex s_secretNameRegex = new("SecretName=(?<Value>[^;]+)(;|$)", RegexOptions.Compiled);

    private static readonly Regex s_secretVersionRegex = new(
        "SecretVersion=(?<Value>[^;]+)(;|$)",
        RegexOptions.Compiled);

    /// <summary>Determines if the specified value is a reference to Key Vault.</summary>
    /// <param name="value">The value to verify.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value" /> is a reference to Key Vault; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsKeyVaultReference([NotNullWhen(true)] string? value)
    {
        return value is not null && s_basicKeyVaultReferenceRegex.IsMatch(value);
    }

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
    public static bool TryParse(string? value, [NotNullWhen(true)] out KeyVaultSecretReference? result)
    {
        return TryParse(value, null, out result);
    }

    /// <summary>
    /// Converts the string representation of a Key Vault reference to a <see cref="KeyVaultSecretReference"/> instance.
    /// A return value indicates whether the conversion succeeded.</summary>
    /// <param name="value">A string containing a Key Vault reference.</param>
    /// <param name="defaultVaultNameOrUri">Default Key Vault name or URI when it is not defined.</param>
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
    public static bool TryParse(
        string? value,
        string? defaultVaultNameOrUri,
        [NotNullWhen(true)] out KeyVaultSecretReference? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        // Determine if the secret value is attempting to use a key vault reference
        string? referenceString = s_basicKeyVaultReferenceRegex.MatchAndGetGroupValue(value, "ReferenceString");
        if (referenceString is null)
        {
            result = null;
            return false;
        }

        try
        {
            result = ParseVaultReference(referenceString, defaultVaultNameOrUri);
            return result is not null;
        }
        catch (Exception e) when (e is ArgumentException or UriFormatException)
        {
            result = null;
            return false;
        }
    }

    private static KeyVaultSecretReference? ParseVaultReference(string vaultReference, string? defaultVaultNameOrUri)
    {
        string? secretUriString = s_secretUriRegex.MatchAndGetGroupValue(vaultReference, "Value");
        if (!string.IsNullOrEmpty(secretUriString))
        {
            if (!Uri.TryCreate(secretUriString, UriKind.Absolute, out Uri? secretUri))
            {
                return null;
            }

            if (!KeyVaultSecretIdentifier.TryCreate(secretUri, out KeyVaultSecretIdentifier secretIdentifier))
            {
                return null;
            }

            return new KeyVaultSecretReference(
                VaultUri: secretIdentifier.VaultUri,
                Name: secretIdentifier.Name,
                Version: secretIdentifier.Version);
        }

        string? vaultName = s_vaultNameRegex.MatchAndGetGroupValue(vaultReference, "Value");
        string? secretName = s_secretNameRegex.MatchAndGetGroupValue(vaultReference, "Value");
        string? version = s_secretVersionRegex.MatchAndGetGroupValue(vaultReference, "Value");
        Uri? vaultUri = ResolveVaultUri(vaultName, defaultVaultNameOrUri);

        if (vaultUri is not null && !string.IsNullOrWhiteSpace(secretName))
        {
            return new KeyVaultSecretReference(
                VaultUri: vaultUri,
                Name: secretName!,
                Version: version);
        }

        return null;
    }

    private static Uri? ResolveVaultUri(string? vaultName, string? defaultVaultNameOrUri)
    {
        if (!string.IsNullOrWhiteSpace(vaultName))
        {
            return new Uri($"https://{vaultName}.{VaultUriSuffix}");
        }

        if (string.IsNullOrWhiteSpace(defaultVaultNameOrUri))
        {
            return null;
        }

        if (Uri.TryCreate(defaultVaultNameOrUri, UriKind.Absolute, out Uri? vaultUri))
        {
            return vaultUri;
        }

        return new Uri($"https://{defaultVaultNameOrUri}.{VaultUriSuffix}");
    }
}
