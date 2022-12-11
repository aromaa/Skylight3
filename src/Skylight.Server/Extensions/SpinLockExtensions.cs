using System.Runtime.CompilerServices;

namespace Skylight.Server.Extensions;

internal static class SpinLockExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryEnter(ref this SpinLock spinLock)
	{
		bool entered = false;

		spinLock.TryEnter(ref entered);

		return entered;
	}
}
