using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Raiqub.AzureKeyVaultReference.Configuration.Tests.BaseProvider;
using Raiqub.AzureKeyVaultReference.Configuration.WrapProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests.WrapProvider;

public class AzureKeyVaultReferenceWrapProviderTest : AzureKeyVaultReferenceBaseProviderTest
{
    public AzureKeyVaultReferenceWrapProviderTest()
        : base(
            (builder, options, manager) => builder.Add(
                new AzureKeyVaultReferenceWrapSource(CreateInnerConfiguration(), options, manager)))
    {
    }

    private static IConfigurationRoot CreateInnerConfiguration()
    {
        var configurationSource = new MemoryConfigurationSource { InitialData = ConfigurationData };
        return ((IConfigurationBuilder)new ConfigurationManager()).Add(configurationSource).Build();
    }
}
