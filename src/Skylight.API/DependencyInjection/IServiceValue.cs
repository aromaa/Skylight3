namespace Skylight.API.DependencyInjection;

public interface IServiceValue<out T>
{
	public T Value { get; }
}
