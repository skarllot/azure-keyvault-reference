#if !NET6_0_OR_GREATER

// ReSharper disable CheckNamespace
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    internal CallerArgumentExpressionAttribute(string parameterName)
    {
    }
}

#endif
