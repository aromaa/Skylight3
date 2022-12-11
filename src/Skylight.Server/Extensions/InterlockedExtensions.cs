using System.Runtime.CompilerServices;

namespace Skylight.Server.Extensions;

internal static class InterlockedExtensions
{
	public static T Exchange<T>(ref T location, T value)
		where T : struct, Enum
	{
		if (Unsafe.SizeOf<T>() == sizeof(uint))
		{
			uint original = Interlocked.Exchange(ref Unsafe.As<T, uint>(ref location), Unsafe.As<T, uint>(ref value));

			return Unsafe.As<uint, T>(ref original);
		}

		throw new NotSupportedException();
	}

	public static T CompareExchange<T>(ref T location, T value, T comparand)
		where T : struct, Enum
	{
		if (Unsafe.SizeOf<T>() == sizeof(uint))
		{
			uint original = Interlocked.CompareExchange(ref Unsafe.As<T, uint>(ref location), Unsafe.As<T, uint>(ref value), Unsafe.As<T, uint>(ref comparand));

			return Unsafe.As<uint, T>(ref original);
		}

		throw new NotSupportedException();
	}
}
