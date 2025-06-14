using AwesomeAssertions;

namespace Raiqub.AzureKeyVaultReference.Tests;

public class KeyVaultSecretReferenceTest
{
    [Theory]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret/)",
        "https://myvault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)",
        "https://myvault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/version)",
        "https://sampleurl/",
        "mysecret",
        "version")]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/version;)",
        "https://sampleurl/",
        "mysecret",
        "version")] // with semicolon at the end
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/)",
        "https://sampleurl/",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret)",
        "https://samplevault.vault.azure.net/",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;)",
        "https://samplevault.vault.azure.net/",
        "mysecret",
        null)] // with semicolon at the end
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;SecretVersion=secretVersion)",
        "https://samplevault.vault.azure.net/",
        "mysecret",
        "secretVersion")]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret;VaultName=sampleVault;SecretVersion=secretVersion)",
        "https://samplevault.vault.azure.net/",
        "mysecret",
        "secretVersion")] // different order
    public void ShouldParse(string input, string uri, string secret, string? version)
    {
        var keyVaultSecretReference1 = KeyVaultSecretReference.Parse(input);
        bool isParsed = KeyVaultSecretReference.TryParse(input, out KeyVaultSecretReference? keyVaultSecretReference2);

        keyVaultSecretReference1.VaultUri.Should().Be(uri);
        keyVaultSecretReference1.Name.Should().Be(secret);
        keyVaultSecretReference1.Version.Should().Be(version);

        isParsed.Should().BeTrue();
        keyVaultSecretReference2.Should().NotBeNull();
        keyVaultSecretReference2!.VaultUri.Should().Be(uri);
        keyVaultSecretReference2.Name.Should().Be(secret);
        keyVaultSecretReference2.Version.Should().Be(version);
    }

    [Theory]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret/)",
        "othervault",
        "https://myvault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)",
        "othervault",
        "https://myvault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)",
        "https://othervault.vault.azure.net",
        "https://myvault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret)",
        "othervault",
        "https://othervault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret)",
        "https://othervault.vault.azure.net",
        "https://othervault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret)",
        "https://othervault.vault.azure.net/",
        "https://othervault.vault.azure.net",
        "mysecret",
        null)]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret)",
        "https://othervault.vault.azure.net:443",
        "https://othervault.vault.azure.net",
        "mysecret",
        null)]
    public void ShouldParseWithDefaultVault(
        string input,
        string defaultVault,
        string uri,
        string secret,
        string? version)
    {
        var keyVaultSecretReference1 = KeyVaultSecretReference.Parse(input, defaultVault);
        bool isParsed = KeyVaultSecretReference.TryParse(
            input,
            defaultVault,
            out KeyVaultSecretReference? keyVaultSecretReference2);

        keyVaultSecretReference1.VaultUri.Should().Be(uri);
        keyVaultSecretReference1.Name.Should().Be(secret);
        keyVaultSecretReference1.Version.Should().Be(version);

        isParsed.Should().BeTrue();
        keyVaultSecretReference2.Should().NotBeNull();
        keyVaultSecretReference2!.VaultUri.Should().Be(uri);
        keyVaultSecretReference2.Name.Should().Be(secret);
        keyVaultSecretReference2.Version.Should().Be(version);
    }

    [Theory]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret/)",
        "https://myvault.vault.azure.net/secrets/mysecret")]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)",
        "https://myvault.vault.azure.net/secrets/mysecret")]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/version)",
        "https://sampleurl/secrets/mysecret/version")]
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/version;)",
        "https://sampleurl/secrets/mysecret/version")] // with semicolon at the end
    [InlineData(
        "@Microsoft.KeyVault(SecretUri=https://sampleurl/secrets/mysecret/)",
        "https://sampleurl/secrets/mysecret")]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret)",
        "https://samplevault.vault.azure.net/secrets/mysecret")]
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;)",
        "https://samplevault.vault.azure.net/secrets/mysecret")] // with semicolon at the end
    [InlineData(
        "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;SecretVersion=secretVersion)",
        "https://samplevault.vault.azure.net/secrets/mysecret/secretVersion")]
    [InlineData(
        "@Microsoft.KeyVault(SecretName=mysecret;VaultName=sampleVault;SecretVersion=secretVersion)",
        "https://samplevault.vault.azure.net/secrets/mysecret/secretVersion")] // different order
    public void ShouldToString(string input, string output)
    {
        var keyVaultSecretReference = KeyVaultSecretReference.Parse(input);
        string result = keyVaultSecretReference.ToString();

        result.Should().Be(output);
    }
}
