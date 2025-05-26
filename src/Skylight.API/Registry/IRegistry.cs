using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Registry;

public interface IRegistry
{
	public ResourceLocation Location { get; }
}

public interface IRegistry<T> : IRegistry
	where T : notnull
{
	public RegistryType<T> Type { get; }
	public IEnumerable<T> Values { get; }

	public T Value(ResourceKey key);
	public bool TryGetValue(ResourceKey key, [NotNullWhen(true)] out T? value);

	ResourceLocation IRegistry.Location => this.Type.Location;
}
