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

    [Benchmark]
    public bool IsKeyVaultReferenceWhenItIsNot()
    {
        return KeyVaultReferenceParser.IsKeyVaultReference("34fec47e-0f54-4919-a6ee-f5b25d6eed01");
    }

    [Benchmark]
    public KeyVaultSecretReference? TryParseWithName()
    {
        KeyVaultReferenceParser.TryParse(
            "@Microsoft.KeyVault(VaultName=sampleVault;SecretName=mysecret;SecretVersion=secretVersion)",
            out var result);

        return result;
    }

    [Benchmark]
    public KeyVaultSecretReference? TryParseWithUri()
    {
        KeyVaultReferenceParser.TryParse(
            "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/mysecret/version)",
            out var result);

        return result;
    }
}
