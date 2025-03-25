using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Havensread.ServiceDefaults;

public static class ThrowHelper
{
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }

        [DoesNotReturn]
        static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);
    }

    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string? message = null)
    {
        if (condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    [DoesNotReturn]
    public static void Throw(string? message = null) => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void Throw<TException>(string? message = null) where TException : Exception, new()
    {
        if (message is null)
        {
            throw new TException();
        }
        throw (TException)Activator.CreateInstance(typeof(TException), message)!;
    }
}
