using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration;

internal sealed class AzureKeyVaultReferenceConfigurationSource : IConfigurationSource
{
    private readonly TokenCredential? _credential;

    public AzureKeyVaultReferenceConfigurationSource(TokenCredential? credential = null)
    {
        _credential = credential;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must be of type ConfigurationManager");
        }

        return new AzureKeyVaultReferenceConfigurationProvider(root, _credential);
    }
}
