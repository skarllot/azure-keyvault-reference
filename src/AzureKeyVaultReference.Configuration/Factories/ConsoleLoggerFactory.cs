using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Raiqub.AzureKeyVaultReference.Configuration.Factories;

internal static class ConsoleLoggerFactory
{
    public static ILoggerFactory Create(AzureKeyVaultReferenceOptions options)
    {
        var loggerProvider =
            new ConsoleLoggerProvider(new StaticOptionsMonitor<ConsoleLoggerOptions>(options.LoggerOptions));

        return new LoggerFactory([loggerProvider]);
    }

    private sealed class StaticOptionsMonitor<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        T> : IOptionsMonitor<T>
    {
        public StaticOptionsMonitor(T currentValue) => CurrentValue = currentValue;

        public T Get(string? name) => CurrentValue;

        public IDisposable OnChange(Action<T, string> listener) => new NullDisposable();

        public T CurrentValue { get; }
    }

    private sealed class NullDisposable : IDisposable
    {
        public void Dispose()
        {
            // Does nothing
        }
    }
}
