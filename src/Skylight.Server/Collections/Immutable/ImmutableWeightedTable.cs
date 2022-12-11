namespace Skylight.Server.Collections.Immutable;

internal static class ImmutableWeightedTable
{
	internal static ImmutableWeightedTable<T>.Builder CreateBuilder<T>()
		where T : notnull
	{
		return new ImmutableWeightedTable<T>.Builder();
	}
}
