using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Raiqub.AzureKeyVaultReference.Configuration.Factories;

internal static class MemoryCacheFactory
{
    public static IMemoryCache Create(AzureKeyVaultReferenceOptions options, ILoggerFactory loggerFactory)
    {
        var cacheOptions = new MemoryCacheOptions();
        cacheOptions.SizeLimit = options.CacheSize;
        cacheOptions.ExpirationScanFrequency = options.CacheRetentionTime / 5;

        return new MemoryCache(cacheOptions, loggerFactory);
    }
}
