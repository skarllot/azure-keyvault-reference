using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;

public sealed class AzureKeyVaultReferenceRecursiveSource : IConfigurationSource
{
    private readonly AzureKeyVaultReferenceOptions _options;

    public AzureKeyVaultReferenceRecursiveSource(AzureKeyVaultReferenceOptions options)
    {
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must be of type ConfigurationManager");
        }

        return new AzureKeyVaultReferenceRecursiveProvider(root, _options, new KeyVaultReferencesManager());
    }
}
