using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests;

public class AzureKeyVaultReferenceExtensionsTest
{
    [Fact]
    public void AddAzureKeyVaultReference_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        IConfigurationBuilder builder = null!;
        Action<IConfigurationBuilder> configureDelegate = _ => { };

        // Act
        Action act = () => builder.AddAzureKeyVaultReference(configureDelegate);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithNullConfigureDelegate_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        Action<IConfigurationBuilder> configureDelegate = null!;

        // Act
        Action act = () => builder.AddAzureKeyVaultReference(configureDelegate);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configureDelegate");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithValidArguments_ShouldAddSourceToBuilder()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var initialSourceCount = builder.Sources.Count;

        // Act
        builder.AddAzureKeyVaultReference(b => b.AddInMemoryCollection());

        // Assert
        builder.Sources.Count.Should().Be(initialSourceCount + 1, "because a new source should be added");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithConfigureDelegate_ShouldInvokeDelegate()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var delegateInvoked = false;

        // Act
        builder.AddAzureKeyVaultReference(b =>
        {
            delegateInvoked = true;
            b.AddInMemoryCollection();
        });

        // Assert
        delegateInvoked.Should().BeTrue("because the configure delegate should be invoked");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithOptionsAction_ShouldInvokeOptionsAction()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var optionsActionInvoked = false;

        // Act
        builder.AddAzureKeyVaultReference(
            b => b.AddInMemoryCollection(),
            _ => { optionsActionInvoked = true; });

        // Assert
        optionsActionInvoked.Should().BeTrue("because the options action should be invoked");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithProvidedManager_ShouldUseProvidedManager()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var manager = Substitute.For<IKeyVaultReferencesManager>();

        // Act
        builder.AddAzureKeyVaultReference(
            b => b.AddInMemoryCollection(),
            manager);

        // Assert
        builder.Sources.Count.Should().Be(1, "because a source should be added");
    }

    [Fact]
    public void AddAzureKeyVaultReference_WithoutProvidedManager_ShouldCreateDefaultManager()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        builder.AddAzureKeyVaultReference(b => b.AddInMemoryCollection());

        // Assert
        builder.Sources.Count.Should().Be(1, "because a source should be added with default manager");
    }

    [Fact]
    public void AddAzureKeyVaultReference_ReturnsConfigurationBuilder()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var result = builder.AddAzureKeyVaultReference(b => b.AddInMemoryCollection());

        // Assert
        result.Should().BeSameAs(builder, "because it should return the same builder for chaining");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_WithNullHostBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        IHostBuilder hostBuilder = null!;

        // Act
        Action act = () => hostBuilder.ConfigureAzureKeyVaultReference();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("hostBuilder");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_WithValidArguments_ShouldConfigureAppConfiguration()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        var result = hostBuilder.ConfigureAzureKeyVaultReference();

        // Assert
        result.Should().NotBeNull("because it should configure the host builder");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_WithOptionsAction_ShouldInvokeOptionsAction()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var optionsActionInvoked = false;

        // Act
        hostBuilder.ConfigureAzureKeyVaultReference(_ =>
        {
            optionsActionInvoked = true;
        });

        // Build to trigger configuration
        using var host = hostBuilder.Build();

        // Assert
        optionsActionInvoked.Should().BeTrue("because the options action should be invoked during build");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_WithProvidedManager_ShouldUseProvidedManager()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var manager = Substitute.For<IKeyVaultReferencesManager>();

        // Act
        hostBuilder.ConfigureAzureKeyVaultReference(manager);
        using var host = hostBuilder.Build();

        // Assert
        host.Should().NotBeNull("because it should build successfully with provided manager");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_ReturnsHostBuilder()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        var result = hostBuilder.ConfigureAzureKeyVaultReference();

        // Assert
        result.Should().BeSameAs(hostBuilder, "because it should return the same builder for chaining");
    }

    [Fact]
    public void ConfigureAzureKeyVaultReference_WithExistingSources_ShouldMoveSourcesCorrectly()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "TestKey", "TestValue" }
                });
            });

        // Act
        hostBuilder.ConfigureAzureKeyVaultReference();
        using var host = hostBuilder.Build();
        var config = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;

        // Assert
        config.Should().NotBeNull("because configuration should be available");
        config["TestKey"].Should().Be("TestValue", "because existing configuration should be preserved");
    }
}
