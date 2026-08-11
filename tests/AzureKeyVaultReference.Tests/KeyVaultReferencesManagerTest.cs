using AwesomeAssertions;
using Azure;
using Azure.Core;
using NSubstitute;

namespace Raiqub.AzureKeyVaultReference.Tests;

public class KeyVaultReferencesManagerTest
{
    private readonly TokenCredential _tokenCredential;
    private readonly KeyVaultReferencesManager _manager;

    public KeyVaultReferencesManagerTest()
    {
        _tokenCredential = Substitute.For<TokenCredential>();
        _manager = new KeyVaultReferencesManager(_tokenCredential);
    }

    [Fact]
    public void GivenASecretReferenceWhenTokenIsInvalidThenThrow()
    {
        var requestFailedException = new RequestFailedException(401, "Unauthorized");
        using var manager = new KeyVaultReferencesManager(
            _tokenCredential,
            (_, _) => throw new AggregateException(requestFailedException));

        Action getSecretValue = () => manager.GetSecretValue(
            KeyVaultSecretReference.Parse("@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)"));

        getSecretValue.Should().ThrowExactly<RequestFailedException>().Where(e => e.Status == requestFailedException.Status);
    }

    [Fact]
    public void ShouldDispose()
    {
        Action dispose = () => _manager.Dispose();

        dispose.Should().NotThrow();
    }
}
