using Azure;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests.BaseProvider;

public abstract class AzureKeyVaultReferenceBaseProviderTest : IDisposable
{
    private static readonly Dictionary<string, string> s_configurationData = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Key1", "Value1" },
        { "Key2", "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;)" },
        { "Key3:Sub1", "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret2;)" },
        { "Key3:Sub2", "Value2" },
        { "Key3:Sub3", "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)" }
    };

    private readonly Mock<IKeyVaultReferencesManager> _mockKeyVaultManager;
    private readonly ConfigurationManager _configurationManager;

    protected AzureKeyVaultReferenceBaseProviderTest(
        Action<IConfigurationBuilder, AzureKeyVaultReferenceOptions, IKeyVaultReferencesManager> builder)
    {
        _mockKeyVaultManager = new Mock<IKeyVaultReferencesManager>();

        _configurationManager = new ConfigurationManager();

        builder(_configurationManager, new AzureKeyVaultReferenceOptions(), _mockKeyVaultManager.Object);
    }

    protected static IReadOnlyDictionary<string, string> ConfigurationData => s_configurationData;

    [Fact]
    public void GivenKeyWhenValueIsNotKeyVaultReferenceThenReturnsOriginalValue()
    {
        string response1 = _configurationManager["Key1"];
        string response2 = _configurationManager["Key1"];
        string response3 = _configurationManager["Key1"];

        response1.Should().Be("Value1");
        response2.Should().Be("Value1");
        response3.Should().Be("Value1");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Never);
    }

    [Fact]
    public void GivenKeyWhenValueIsKeyVaultReferenceThenResolveSecret()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "value2"), Mock.Of<Response>()))
            .Verifiable();

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("value2");
        response2.Should().Be("value2");
        response3.Should().Be("value2");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Once);
    }

    [Fact]
    public void GivenKeyWhenValueIsKeyVaultReferenceButGetFailsThenReturnsOriginalValue()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret")))
            .Throws(() => new RequestFailedException("Failed"))
            .Verifiable();

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be(s_configurationData["Key2"]);
        response2.Should().Be(s_configurationData["Key2"]);
        response3.Should().Be(s_configurationData["Key2"]);

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Exactly(3));
    }

    [Fact]
    public void GivenKeyWhenValueIsSetToKeyVaultReferenceThenResolveSecret()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "value2"), Mock.Of<Response>()))
            .Verifiable();
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "other")))
            .Returns(Response.FromValue(new KeyVaultSecret("other", "othervalue"), Mock.Of<Response>()))
            .Verifiable();

        string response1 = _configurationManager["Key2"];

        _configurationManager["Key2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=other;)";

        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("value2");
        response2.Should().Be("othervalue");
        response3.Should().Be("othervalue");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Exactly(2));
    }

    [Fact]
    public void GivenKeyWhenNonCachedValueIsSetToKeyVaultReferenceThenResolveSecret()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "other")))
            .Returns(Response.FromValue(new KeyVaultSecret("other", "othervalue"), Mock.Of<Response>()))
            .Verifiable();

        _configurationManager["Key2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=other;)";

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("othervalue");
        response2.Should().Be("othervalue");
        response3.Should().Be("othervalue");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Once);
    }

    [Fact]
    public void GivenSectionWhenHasKeyVaultReferenceThenResolveSecrets()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret2")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Mock.Of<Response>()))
            .Verifiable();

        var section = _configurationManager.GetSection("Key3");
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "Value2",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Once);
    }

    [Fact]
    public void GivenSectionWhenHasRuntimeSetValueThenReturnCorrectChildren()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret2")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Mock.Of<Response>()))
            .Verifiable();
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "othersecret")))
            .Returns(Response.FromValue(new KeyVaultSecret("othersecret", "othervalue"), Mock.Of<Response>()))
            .Verifiable();

        var section = _configurationManager.GetSection("Key3");
        section["Test"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=othersecret;)";
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3", "Test");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "Value2",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)",
            "othervalue");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Exactly(2));
    }

    [Fact]
    public void GivenSectionWhenHasRuntimeOverridenValueThenReturnCorrectChildren()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret2")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Mock.Of<Response>()))
            .Verifiable();
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "othersecret")))
            .Returns(Response.FromValue(new KeyVaultSecret("othersecret", "othervalue"), Mock.Of<Response>()))
            .Verifiable();

        var section = _configurationManager.GetSection("Key3");
        section["Sub2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=othersecret;)";
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "othervalue",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)");

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Exactly(2));
    }

    [Fact]
    public void GivenRootSectionWhenHasKeyVaultReferenceThenResolveSecrets()
    {
        _mockKeyVaultManager
            .Setup(m => m.GetSecretValue(It.Is<KeyVaultSecretReference>(r => r.Name == "mysecret")))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "Value2"), Mock.Of<Response>()))
            .Verifiable();

        var children = _configurationManager.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Key1", "Key2", "Key3");
        children.Select(i => i.Value).Should().Equal("Value1", "Value2", null);

        _mockKeyVaultManager
            .Verify(m => m.GetSecretValue(It.IsAny<KeyVaultSecretReference>()), Times.Once);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _configurationManager.Dispose();
        }
    }
}
