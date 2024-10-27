#if !NET6_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Raiqub.AzureKeyVaultReference.Configuration.Polyfill;

internal static class Requires
{
    public static T NotNull<T>([NotNull] T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class // ensures value-types aren't passed to a null checking method
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return value;
    }
}

#endif
