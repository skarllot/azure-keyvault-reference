using Azure;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

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

    private readonly IKeyVaultReferencesManager _keyVaultManager;
    private readonly ConfigurationManager _configurationManager;

    protected AzureKeyVaultReferenceBaseProviderTest(
        Action<IConfigurationBuilder, AzureKeyVaultReferenceOptions, IKeyVaultReferencesManager> builder)
    {
        _keyVaultManager = Substitute.For<IKeyVaultReferencesManager>();

        _configurationManager = new ConfigurationManager();

        builder(_configurationManager, new AzureKeyVaultReferenceOptions(), _keyVaultManager);
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

        _keyVaultManager.Received(0).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenKeyWhenValueIsKeyVaultReferenceThenResolveSecret()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "value2"), Substitute.For<Response>()));

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("value2");
        response2.Should().Be("value2");
        response3.Should().Be("value2");

        _keyVaultManager.Received(1).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenKeyWhenValueIsKeyVaultReferenceButGetFailsThenReturnsOriginalValue()
    {
        _keyVaultManager
            .When(m => m.GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret")))
            .Do(_ => throw new RequestFailedException("Failed"));

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be(s_configurationData["Key2"]);
        response2.Should().Be(s_configurationData["Key2"]);
        response3.Should().Be(s_configurationData["Key2"]);

        _keyVaultManager.Received(3).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenKeyWhenValueIsSetToKeyVaultReferenceThenResolveSecret()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "value2"), Substitute.For<Response>()));
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "other"))
            .Returns(Response.FromValue(new KeyVaultSecret("other", "othervalue"), Substitute.For<Response>()));

        string response1 = _configurationManager["Key2"];

        _configurationManager["Key2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=other;)";

        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("value2");
        response2.Should().Be("othervalue");
        response3.Should().Be("othervalue");

        _keyVaultManager.Received(2).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenKeyWhenNonCachedValueIsSetToKeyVaultReferenceThenResolveSecret()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "other"))
            .Returns(Response.FromValue(new KeyVaultSecret("other", "othervalue"), Substitute.For<Response>()));

        _configurationManager["Key2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=other;)";

        string response1 = _configurationManager["Key2"];
        string response2 = _configurationManager["Key2"];
        string response3 = _configurationManager["Key2"];

        response1.Should().Be("othervalue");
        response2.Should().Be("othervalue");
        response3.Should().Be("othervalue");

        _keyVaultManager.Received(1).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenSectionWhenHasKeyVaultReferenceThenResolveSecrets()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret2"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Substitute.For<Response>()));

        var section = _configurationManager.GetSection("Key3");
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "Value2",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)");

        _keyVaultManager.Received(1).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenSectionWhenHasRuntimeSetValueThenReturnCorrectChildren()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret2"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Substitute.For<Response>()));
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "othersecret"))
            .Returns(Response.FromValue(new KeyVaultSecret("othersecret", "othervalue"), Substitute.For<Response>()));

        var section = _configurationManager.GetSection("Key3");
        section["Test"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=othersecret;)";
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3", "Test");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "Value2",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)",
            "othervalue");

        _keyVaultManager.Received(2).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenSectionWhenHasRuntimeOverridenValueThenReturnCorrectChildren()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(k => k.Name == "mysecret2"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret2", "myvalue"), Substitute.For<Response>()));
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(r => r.Name == "othersecret"))
            .Returns(Response.FromValue(new KeyVaultSecret("othersecret", "othervalue"), Substitute.For<Response>()));

        var section = _configurationManager.GetSection("Key3");
        section["Sub2"] = "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=othersecret;)";
        var children = section.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Sub1", "Sub2", "Sub3");
        children.Select(i => i.Value).Should().Equal(
            "myvalue",
            "othervalue",
            "@Microsoft.KeyVault(https://sampleurl/secrets/mysecret/)");

        _keyVaultManager.Received(2).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
    }

    [Fact]
    public void GivenRootSectionWhenHasKeyVaultReferenceThenResolveSecrets()
    {
        _keyVaultManager
            .GetSecretValue(Arg.Is<KeyVaultSecretReference>(r => r.Name == "mysecret"))
            .Returns(Response.FromValue(new KeyVaultSecret("mysecret", "Value2"), Substitute.For<Response>()));

        var children = _configurationManager.GetChildren().ToArray();

        children.Select(i => i.Key).Should().Equal("Key1", "Key2", "Key3");
        children.Select(i => i.Value).Should().Equal("Value1", "Value2", null);

        _keyVaultManager.Received(1).GetSecretValue(Arg.Any<KeyVaultSecretReference>());
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
