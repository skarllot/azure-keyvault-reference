using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.WrapProvider;

public class AzureKeyVaultReferenceWrapSource : IConfigurationSource
{
    private readonly IConfigurationRoot _internalConfiguration;
    private readonly AzureKeyVaultReferenceOptions _options;

    public AzureKeyVaultReferenceWrapSource(
        IConfigurationRoot internalConfiguration,
        AzureKeyVaultReferenceOptions options)
    {
        _internalConfiguration = internalConfiguration;
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AzureKeyVaultReferenceWrapProvider(
            _internalConfiguration,
            _options,
            new KeyVaultReferencesManager());
    }
}
