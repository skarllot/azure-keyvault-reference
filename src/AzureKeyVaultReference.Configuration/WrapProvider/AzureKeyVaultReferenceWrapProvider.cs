using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Raiqub.AzureKeyVaultReference.Configuration.BaseProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.WrapProvider;

public sealed class AzureKeyVaultReferenceWrapProvider : AzureKeyVaultReferenceBaseProvider, IConfigurationProvider
{
    private readonly IConfigurationRoot _configuration;

    public AzureKeyVaultReferenceWrapProvider(
        IConfigurationRoot configuration,
        AzureKeyVaultReferenceOptions options,
        IKeyVaultReferencesManager keyVault)
        : base(options, keyVault)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public bool TryGet(string key, out string? value)
    {
        return TryGetInternal(key, _configuration, out value);
    }

    /// <inheritdoc />
    public void Set(string key, string? value)
    {
        SetMemoryValue(key, value);
    }

    /// <inheritdoc />
    public IChangeToken? GetReloadToken()
    {
        return null;
    }

    /// <inheritdoc />
    public void Load()
    {
        LoadMemoryValues();
    }

    /// <inheritdoc />
    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return GetChildKeysInternal(_configuration, earlierKeys, parentPath);
    }
}
