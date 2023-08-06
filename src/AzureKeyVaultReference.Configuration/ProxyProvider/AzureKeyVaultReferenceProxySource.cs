using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;

public class AzureKeyVaultReferenceProxySource : IConfigurationSource
{
    private readonly IConfigurationRoot _internalConfiguration;
    private readonly AzureKeyVaultReferenceOptions _options;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;

    public AzureKeyVaultReferenceProxySource(
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
        return new AzureKeyVaultReferenceProxyProvider(
            _internalConfiguration,
            _options,
            _keyVaultReferencesManager);
    }
}
