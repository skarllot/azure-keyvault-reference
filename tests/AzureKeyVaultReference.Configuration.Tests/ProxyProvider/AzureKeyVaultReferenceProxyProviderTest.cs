using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Raiqub.AzureKeyVaultReference.Configuration.ProxyProvider;
using Raiqub.AzureKeyVaultReference.Configuration.Tests.BaseProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests.ProxyProvider;

public class AzureKeyVaultReferenceProxyProviderTest : AzureKeyVaultReferenceBaseProviderTest
{
    public AzureKeyVaultReferenceProxyProviderTest()
        : base(
            (builder, options, manager) => builder.Add(
                new AzureKeyVaultReferenceProxySource(CreateInnerConfiguration(), options, manager)))
    {
    }

    private static IConfigurationRoot CreateInnerConfiguration()
    {
        var configurationSource = new MemoryConfigurationSource { InitialData = ConfigurationData };
        return ((IConfigurationBuilder)new ConfigurationManager()).Add(configurationSource).Build();
    }
}
