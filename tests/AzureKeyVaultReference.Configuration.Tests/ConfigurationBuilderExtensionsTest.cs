using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests;

public class ConfigurationBuilderExtensionsTest
{
    [Fact]
    public void MoveSourcesToNewConfiguration_WithSingleSource_ShouldMoveSourceToNewConfiguration()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var source = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            }
        };
        builder.Add(source);

        // Act
        IConfigurationRoot result = builder.MoveSourcesToNewConfiguration();

        // Assert
        result.Should().NotBeNull();
        result["Key1"].Should().Be("Value1");
        result["Key2"].Should().Be("Value2");
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_WithMultipleSources_ShouldMoveAllSourcesToNewConfiguration()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var source1 = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key1", "Value1" }
            }
        };
        var source2 = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key2", "Value2" }
            }
        };
        builder.Add(source1);
        builder.Add(source2);

        // Act
        IConfigurationRoot result = builder.MoveSourcesToNewConfiguration();

        // Assert
        result.Should().NotBeNull();
        result["Key1"].Should().Be("Value1");
        result["Key2"].Should().Be("Value2");
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_ShouldClearOriginalBuilderSources()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var source = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key1", "Value1" }
            }
        };
        builder.Add(source);
        int initialSourceCount = builder.Sources.Count;

        // Act
        builder.MoveSourcesToNewConfiguration();

        // Assert
        initialSourceCount.Should().Be(1, "because one source was added");
        builder.Sources.Should().BeEmpty("because sources should be cleared");
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_WithEmptyBuilder_ShouldReturnEmptyConfiguration()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        IConfigurationRoot result = builder.MoveSourcesToNewConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.AsEnumerable().Should().BeEmpty();
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_AfterMove_OriginalBuilderCanBeReused()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var source1 = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key1", "Value1" }
            }
        };
        builder.Add(source1);

        // Act
        IConfigurationRoot result1 = builder.MoveSourcesToNewConfiguration();

        var source2 = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key2", "Value2" }
            }
        };
        builder.Add(source2);
        IConfigurationRoot result2 = builder.Build();

        // Assert
        result1["Key1"].Should().Be("Value1");
        result1["Key2"].Should().BeNull();

        result2["Key1"].Should().BeNull();
        result2["Key2"].Should().Be("Value2");
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_ShouldClearOriginalBuilderProperties()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        var source = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "Key1", "Value1" }
            }
        };
        builder.Add(source);
        builder.Properties["TestProperty"] = "TestValue";

        // Act
        builder.MoveSourcesToNewConfiguration();

        // Assert
        builder.Properties.Should().BeEmpty();
    }

    [Fact]
    public void MoveSourcesToNewConfiguration_WithPropertiesAndNoSources_ShouldStillCopyProperties()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        builder.Properties["Property1"] = "Value1";
        builder.Properties["Property2"] = 123;

        // Act
        IConfigurationRoot result = builder.MoveSourcesToNewConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.AsEnumerable().Should().BeEmpty("because no sources were added");
    }
}
