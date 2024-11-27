namespace Skylight.API.Collections.Cache;

public interface ICacheReference<out T> : IDisposable
{
	public T Value { get; }

	public ICacheReference<T> Retain();
}
