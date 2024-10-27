using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;

/// <summary>
/// Represents a configuration source that recursively resolves Key Vault references in the configuration.
/// </summary>
public sealed class AzureKeyVaultReferenceRecursiveSource : IConfigurationSource
{
    private readonly AzureKeyVaultReferenceOptions _options;
    private readonly IKeyVaultReferencesManager _keyVaultReferencesManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultReferenceRecursiveSource"/> class with the
    /// specified options and Key Vault references manager.
    /// </summary>
    /// <param name="options">The configuration options for the source.</param>
    /// <param name="keyVaultReferencesManager">The manager responsible for handling Key Vault references.</param>
    public AzureKeyVaultReferenceRecursiveSource(
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVaultReferencesManager)
    {
        _options = options;
        _keyVaultReferencesManager = keyVaultReferencesManager;
    }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must implement type IConfigurationRoot");
        }

        return new AzureKeyVaultReferenceRecursiveProvider(root, _options, _keyVaultReferencesManager);
    }
}
