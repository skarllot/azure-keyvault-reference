using AwesomeAssertions;

namespace Raiqub.AzureKeyVaultReference.Tests;

public class KeyVaultReferenceParserTest
{
    [Fact]
    public void IsKeyVaultReference_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = KeyVaultReferenceParser.IsKeyVaultReference(null);

        // Assert
        result.Should().BeFalse("because null is not a valid reference");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("regular string")]
    [InlineData("@Microsoft.KeyVault")]
    [InlineData("@Microsoft.KeyVault(")]
    [InlineData("Microsoft.KeyVault(SecretUri=test)")]
    [InlineData("@Microsoft.KeyVault SecretUri=test)")]
    [InlineData("@microsoft.keyvault(SecretUri=test)")]
    public void IsKeyVaultReference_WithInvalidFormat_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.IsKeyVaultReference(value);

        // Assert
        result.Should().BeFalse($"because '{value}' is not a valid Key Vault reference format");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault()")]
    [InlineData("@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/test)")]
    [InlineData("@Microsoft.KeyVault(VaultName=vault;SecretName=secret)")]
    public void IsKeyVaultReference_WithValidFormat_ShouldReturnTrue(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.IsKeyVaultReference(value);

        // Assert
        result.Should().BeTrue($"because '{value}' matches the Key Vault reference format");
    }

    [Fact]
    public void TryParse_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(null, out var reference);

        // Assert
        result.Should().BeFalse("because null cannot be parsed");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("")]
    [InlineData("regular string")]
    [InlineData("@Microsoft.KeyVault")]
    [InlineData("Microsoft.KeyVault(SecretUri=test)")]
    public void TryParse_WithInvalidFormat_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' does not match the expected format");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault()")]
    [InlineData("@Microsoft.KeyVault(InvalidKey=value)")]
    public void TryParse_WithEmptyOrInvalidContent_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' contains no valid secret information");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(SecretUri=not-a-uri)")]
    [InlineData("@Microsoft.KeyVault(SecretUri=http://)")]
    [InlineData("@Microsoft.KeyVault(SecretUri=)")]
    public void TryParse_WithInvalidSecretUri_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' contains an invalid URI");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(SecretUri=https://www.google.com)")]
    [InlineData("@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net)")]
    [InlineData("@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/)")]
    public void TryParse_WithNonKeyVaultUri_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' is not a valid Key Vault secret URI");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(VaultName=myvault)")]
    [InlineData("@Microsoft.KeyVault(VaultName=myvault;SecretName=)")]
    [InlineData("@Microsoft.KeyVault(VaultName=myvault;SecretName=;SecretVersion=v1)")]
    public void TryParse_WithMissingSecretName_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' is missing a secret name");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(SecretName=mysecret)")]
    [InlineData("@Microsoft.KeyVault(SecretName=mysecret;SecretVersion=v1)")]
    public void TryParse_WithSecretNameButNoVault_ShouldReturnFalse(string value)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeFalse($"because '{value}' has no vault specified and no default vault");
        reference.Should().BeNull("because parsing failed");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(SecretName=mysecret)", "")]
    [InlineData("@Microsoft.KeyVault(SecretName=mysecret)", "   ")]
    public void TryParse_WithSecretNameAndEmptyDefaultVault_ShouldReturnFalse(string value, string defaultVault)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, defaultVault, out var reference);

        // Assert
        result.Should().BeFalse($"because default vault is empty or whitespace");
        reference.Should().BeNull("because parsing failed");
    }

    [Fact]
    public void TryParse_WithSecretNameAndDefaultVault_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(SecretName=mysecret)";
        var defaultVault = "myvault";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, defaultVault, out var reference);

        // Assert
        result.Should().BeTrue("because a default vault is provided");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
    }

    [Fact]
    public void TryParse_WithSecretNameAndDefaultVaultAsUri_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(SecretName=mysecret)";
        var defaultVault = "https://myvault.vault.azure.net";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, defaultVault, out var reference);

        // Assert
        result.Should().BeTrue("because a default vault URI is provided");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
    }

    [Fact]
    public void TryParse_WithValidSecretUri_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret)";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because the secret URI is valid");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
        reference.Version.Should().BeNull("because no version was specified");
    }

    [Fact]
    public void TryParse_WithValidSecretUriAndVersion_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret/v123)";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because the secret URI with version is valid");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
        reference.Version.Should().Be("v123");
    }

    [Fact]
    public void TryParse_WithVaultNameAndSecretName_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because vault name and secret name are provided");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
        reference.Version.Should().BeNull("because no version was specified");
    }

    [Fact]
    public void TryParse_WithVaultNameSecretNameAndVersion_ShouldSucceed()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret;SecretVersion=v1)";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because all required fields are provided");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://myvault.vault.azure.net");
        reference.Name.Should().Be("mysecret");
        reference.Version.Should().Be("v1");
    }

    [Fact]
    public void TryParse_SecretUriTakesPrecedenceOverVaultName()
    {
        // Arrange
        var value = "@Microsoft.KeyVault(SecretUri=https://uri-vault.vault.azure.net/secrets/uri-secret;VaultName=name-vault;SecretName=name-secret)";

        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because SecretUri takes precedence");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be("https://uri-vault.vault.azure.net", "because SecretUri is used");
        reference.Name.Should().Be("uri-secret", "because SecretUri is used");
    }

    [Theory]
    [InlineData("@Microsoft.KeyVault(VaultName=MyVault;SecretName=mysecret)", "https://myvault.vault.azure.net")]
    [InlineData("@Microsoft.KeyVault(VaultName=My-Vault;SecretName=mysecret)", "https://my-vault.vault.azure.net")]
    [InlineData("@Microsoft.KeyVault(VaultName=UPPERCASE;SecretName=mysecret)", "https://uppercase.vault.azure.net")]
    public void TryParse_VaultNameIsCasePreserving(string value, string expectedUri)
    {
        // Act
        var result = KeyVaultReferenceParser.TryParse(value, out var reference);

        // Assert
        result.Should().BeTrue("because the vault name is valid");
        reference.Should().NotBeNull("because parsing succeeded");
        reference!.VaultUri.Should().Be(expectedUri, "because vault name case is preserved");
    }
}