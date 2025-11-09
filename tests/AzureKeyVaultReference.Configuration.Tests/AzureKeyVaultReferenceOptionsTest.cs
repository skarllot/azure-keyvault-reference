using AwesomeAssertions;
using Azure.Core;
using Azure.Identity;
using NSubstitute;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests;

public class AzureKeyVaultReferenceOptionsTest
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new AzureKeyVaultReferenceOptions();

        // Assert
        options.CacheSize.Should().Be(100, "because the default cache size is 100");
        options.CacheRetentionTime.Should().Be(TimeSpan.FromMinutes(30), "because the default retention time is 30 minutes");
        options.LoggerOptions.Should().NotBeNull("because a default logger options should be created");
        options.GetDefaultVaultNameOrUri.Should().NotBeNull("because a default function should be provided");
    }

    [Fact]
    public void Credential_WhenNotSet_ShouldReturnDefaultAzureCredential()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();

        // Act
        var credential = options.Credential;

        // Assert
        credential.Should().NotBeNull("because a default credential should be created");
        credential.Should().BeOfType<DefaultAzureCredential>("because DefaultAzureCredential is the default");
    }

    [Fact]
    public void Credential_WhenSet_ShouldReturnProvidedCredential()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();
        var customCredential = Substitute.For<TokenCredential>();

        // Act
        options.Credential = customCredential;

        // Assert
        options.Credential.Should().BeSameAs(customCredential, "because the custom credential should be returned");
    }

    [Fact]
    public void Credential_WhenAccessedMultipleTimes_ShouldReturnSameInstance()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();

        // Act
        var credential1 = options.Credential;
        var credential2 = options.Credential;

        // Assert
        credential1.Should().BeSameAs(credential2, "because the credential should be cached");
    }

    [Fact]
    public void CacheSize_CanBeSet()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();

        // Act
        options.CacheSize = 200;

        // Assert
        options.CacheSize.Should().Be(200, "because the cache size should be updated");
    }

    [Fact]
    public void CacheRetentionTime_CanBeSet()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();
        var newRetentionTime = TimeSpan.FromHours(1);

        // Act
        options.CacheRetentionTime = newRetentionTime;

        // Assert
        options.CacheRetentionTime.Should().Be(newRetentionTime, "because the retention time should be updated");
    }

    [Fact]
    public void GetDefaultVaultNameOrUri_DefaultFunction_ShouldReturnNull()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();

        // Act
        var result = options.GetDefaultVaultNameOrUri();

        // Assert
        result.Should().BeNull("because the default function returns null");
    }

    [Fact]
    public void GetDefaultVaultNameOrUri_CanBeSet()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions();
        var expectedVaultName = "myVault";

        // Act
        options.GetDefaultVaultNameOrUri = () => expectedVaultName;

        // Assert
        options.GetDefaultVaultNameOrUri().Should().Be(expectedVaultName, "because the custom function should be used");
    }

    [Fact]
    public void CopyFrom_WithNullOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new AzureKeyVaultReferenceOptions { CacheSize = 200 };

        // Act
        Action act = () => options.CopyFrom(null);

        // Assert
        act.Should().NotThrow("because null options should be handled gracefully");
        options.CacheSize.Should().Be(200, "because original values should remain unchanged");
    }

    [Fact]
    public void CopyFrom_WithSourceOptions_ShouldCopyAllProperties()
    {
        // Arrange
        var source = new AzureKeyVaultReferenceOptions
        {
            CacheSize = 500,
            CacheRetentionTime = TimeSpan.FromHours(2),
            GetDefaultVaultNameOrUri = () => "sourceVault"
        };
        var sourceCredential = Substitute.For<TokenCredential>();
        source.Credential = sourceCredential;

        var target = new AzureKeyVaultReferenceOptions();

        // Act
        target.CopyFrom(source);

        // Assert
        target.CacheSize.Should().Be(500, "because cache size should be copied");
        target.CacheRetentionTime.Should().Be(TimeSpan.FromHours(2), "because retention time should be copied");
        target.Credential.Should().BeSameAs(sourceCredential, "because credential should be copied");
        target.GetDefaultVaultNameOrUri().Should().Be("sourceVault", "because the function should be copied");
        target.LoggerOptions.Should().BeSameAs(source.LoggerOptions, "because logger options should be copied");
    }

    [Fact]
    public void CopyFrom_WithSourceOptionsWithoutCredentialSet_ShouldNotCopyCredential()
    {
        // Arrange
        var source = new AzureKeyVaultReferenceOptions
        {
            CacheSize = 500
        };
        // Don't access Credential property on source, so it won't be initialized

        var target = new AzureKeyVaultReferenceOptions();
        var targetCredential = Substitute.For<TokenCredential>();
        target.Credential = targetCredential;

        // Act
        target.CopyFrom(source);

        // Assert
        // Since source._credential is null, target's credential should become null too
        // and when accessed, it will create a new DefaultAzureCredential
        var resultCredential = target.Credential;
        resultCredential.Should().BeOfType<DefaultAzureCredential>("because credential was not set in source");
    }

    [Fact]
    public void CopyFrom_ShouldOverwriteExistingValues()
    {
        // Arrange
        var source = new AzureKeyVaultReferenceOptions
        {
            CacheSize = 300,
            CacheRetentionTime = TimeSpan.FromMinutes(45)
        };

        var target = new AzureKeyVaultReferenceOptions
        {
            CacheSize = 100,
            CacheRetentionTime = TimeSpan.FromMinutes(15)
        };

        // Act
        target.CopyFrom(source);

        // Assert
        target.CacheSize.Should().Be(300, "because source values should overwrite target values");
        target.CacheRetentionTime.Should().Be(TimeSpan.FromMinutes(45), "because source values should overwrite target values");
    }
}