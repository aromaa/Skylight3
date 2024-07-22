namespace Skylight.Server.DependencyInjection;

internal abstract class VersionedServiceSnapshot
{
	internal int Version { get; private protected set; }

	internal abstract class Builder<T>
		where T : VersionedServiceSnapshot
	{
		internal abstract T Build();

		internal abstract Transaction<T> BuildAndStartTransaction(VersionedLoadableServiceBase instance, T current);
	}

	internal abstract class Transaction<T>(T current) : IDisposable
		where T : VersionedServiceSnapshot
	{
		internal T Current { get; } = current;

		internal abstract T Commit(int version);

		public abstract void Dispose();
	}
}
