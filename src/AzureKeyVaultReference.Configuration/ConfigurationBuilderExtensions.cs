using Microsoft.Extensions.Configuration;

namespace Raiqub.AzureKeyVaultReference.Configuration;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationRoot MoveSourcesToNewConfiguration(this IConfigurationBuilder builder)
    {
        var newBuilder = new ConfigurationBuilder();
        foreach (var property in builder.Properties)
        {
            newBuilder.Properties.Add(property.Key, property.Value);
        }

        foreach (var source in builder.Sources)
        {
            newBuilder.Sources.Add(source);
        }

        builder.Sources.Clear();
        return newBuilder.Build();
    }
}
