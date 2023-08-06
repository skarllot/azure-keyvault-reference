using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.WrapProvider;

public class AzureKeyVaultReferenceWrapSource : IConfigurationSource
{
    private readonly IConfigurationRoot _internalConfiguration;
    private readonly AzureKeyVaultReferenceOptions _options;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;

    public AzureKeyVaultReferenceWrapSource(
        IConfigurationRoot internalConfiguration,
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _internalConfiguration = internalConfiguration;
        _options = options;
        _keyVaultReferencesManager = keyVaultReferencesManager;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AzureKeyVaultReferenceWrapProvider(
            _internalConfiguration,
            _options,
            _keyVaultReferencesManager);
    }
}
