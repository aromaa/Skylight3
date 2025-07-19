using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Registry;

public readonly record struct RegistryReference<T>(RegistryType<T> Registry, ResourceKey Key)
	where T : notnull
{
	public T Get(IRegistryHolder holder) => this.Get(holder.Registry(this.Registry));
	public T Get(IRegistry<T> registry) => registry.Value(this.Key);

	public bool TryGet(IRegistryHolder holder, [NotNullWhen(true)] out T? value) => this.TryGet(holder.Registry(this.Registry), out value);
	public bool TryGet(IRegistry<T> registry, [NotNullWhen(true)] out T? value) => registry.TryGetValue(this.Key, out value);
}

public readonly record struct RegistryReference<TValue, TRegistry>(RegistryType<TRegistry> Registry, ResourceKey Key)
	where TValue : TRegistry
	where TRegistry : notnull
{
	public TValue Get(IRegistryHolder holder) => this.Get(holder.Registry(this.Registry));
	public TValue Get(IRegistry<TRegistry> registry) => (TValue)registry.Value(this.Key);

	public bool TryGet(IRegistryHolder holder, [NotNullWhen(true)] out TValue? value) => this.TryGet(holder.Registry(this.Registry), out value);

	public bool TryGet(IRegistry<TRegistry> registry, [NotNullWhen(true)] out TValue? value)
	{
		if (registry.TryGetValue(this.Key, out TRegistry? registryValue))
		{
			value = (TValue)registryValue;
			return true;
		}

		value = default;
		return false;
	}
}
