using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration;

public sealed class AzureKeyVaultReferenceSource : IConfigurationSource
{
    private readonly AzureKeyVaultReferenceOptions _options;

    public AzureKeyVaultReferenceSource(AzureKeyVaultReferenceOptions options)
    {
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must be of type ConfigurationManager");
        }

        return new AzureKeyVaultReferenceProvider(root, _options);
    }
}
