using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Raiqub.AzureKeyVaultReference.Configuration.Factories;

internal static class MemoryCacheFactory
{
    public static IMemoryCache Create(AzureKeyVaultReferenceOptions options, ILoggerFactory loggerFactory)
    {
        var cacheOptions = new MemoryCacheOptions();
        cacheOptions.SizeLimit = options.CacheSize;

#if NET6_0_OR_GREATER || NETSTANDARD2_1
        cacheOptions.ExpirationScanFrequency = options.CacheRetentionTime / 5;

        return new MemoryCache(cacheOptions, loggerFactory);
#else
        cacheOptions.ExpirationScanFrequency =
            TimeSpan.FromTicks((long)Math.Round(options.CacheRetentionTime.Ticks / 5d));

        return new MemoryCache(cacheOptions);
#endif
    }
}
