namespace Skylight.API.Collections.Cache;

public interface ICacheValue<out T> : IDisposable
{
	public T Value { get; }
}
