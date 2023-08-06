using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Raiqub.AzureKeyVaultReference.Configuration.RecursiveProvider;
using Raiqub.AzureKeyVaultReference.Configuration.Tests.BaseProvider;

namespace Raiqub.AzureKeyVaultReference.Configuration.Tests.RecursiveProvider;

public class AzureKeyVaultReferenceRecursiveProviderTest : AzureKeyVaultReferenceBaseProviderTest
{
    public AzureKeyVaultReferenceRecursiveProviderTest()
        : base(
            (builder, options, manager) => builder
                .Add(CreateSource())
                .Add(new AzureKeyVaultReferenceRecursiveSource(options, manager)))
    {
    }

    private static MemoryConfigurationSource CreateSource()
    {
        return new MemoryConfigurationSource { InitialData = ConfigurationData };
    }
}
