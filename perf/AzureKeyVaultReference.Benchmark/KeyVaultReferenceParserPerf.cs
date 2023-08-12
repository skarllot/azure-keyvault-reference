using BenchmarkDotNet.Attributes;
using Raiqub.AzureKeyVaultReference;

namespace AzureKeyVaultReference.Benchmark;

[MemoryDiagnoser]
public class KeyVaultReferenceParserPerf
{
    [Benchmark]
    public bool IsKeyVaultReferenceWhenItIs()
    {
        return KeyVaultReferenceParser.IsKeyVaultReference(
            "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;SecretVersion=secretVersion)");
    }
}
