using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;

public sealed class AzureKeyVaultReferenceRecursiveSource : IConfigurationSource
{
    private readonly AzureKeyVaultReferenceOptions _options;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;

    public AzureKeyVaultReferenceRecursiveSource(
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _options = options;
        _keyVaultReferencesManager = keyVaultReferencesManager;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must be of type ConfigurationManager");
        }

        return new AzureKeyVaultReferenceRecursiveProvider(root, _options, _keyVaultReferencesManager);
    }
}
