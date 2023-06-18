using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raiqub.AzureKeyVaultReference.Configuration;

internal sealed class AzureKeyVaultReferenceConfigurationSource : IConfigurationSource
{
    private readonly IServiceCollection _services;

    public AzureKeyVaultReferenceConfigurationSource(IServiceCollection services)
    {
        _services = services;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not IConfigurationRoot root)
        {
            throw new NotSupportedException("The configuration builder must be of type ConfigurationManager");
        }

        return new AzureKeyVaultReferenceConfigurationProvider(root, _services);
    }
}
