using Azure;
using Azure.Core;
using FluentAssertions;
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
        _tokenCredential
            .GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("token", DateTimeOffset.UtcNow.AddDays(1)));

        Action getSecretValue = () => _manager.GetSecretValue(
            KeyVaultSecretReference.Parse("@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecret)"));

        getSecretValue.Should().ThrowExactly<RequestFailedException>().Where(e => e.Status == 401);
    }

    [Fact]
    public void ShouldDispose()
    {
        Action dispose = () => _manager.Dispose();

        dispose.Should().NotThrow();
    }
}
