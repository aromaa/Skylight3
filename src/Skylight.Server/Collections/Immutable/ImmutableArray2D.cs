namespace Skylight.Server.Collections.Immutable;

internal static class ImmutableArray2D
{
	internal static ImmutableArray2D<T>.Builder CreateBuilder<T>(int width, int height)
	{
		return new ImmutableArray2D<T>.Builder(width, height);
	}
}
