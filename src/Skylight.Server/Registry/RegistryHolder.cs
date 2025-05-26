using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Skylight.API;
using Skylight.API.Registry;

namespace Skylight.Server.Registry;

internal abstract class RegistryHolder(params IEnumerable<IRegistry> registries) : IRegistryHolder
{
	private readonly FrozenDictionary<ResourceLocation, IRegistry> registries = registries.ToFrozenDictionary(v => v.Location, v => v);

	public IRegistry<T> Registry<T>(RegistryType<T> type)
		where T : notnull => (IRegistry<T>)this.registries[type.Location];

	public bool TryGetRegistry<T>(RegistryType<T> type, [NotNullWhen(true)] out IRegistry<T>? registry)
		where T : notnull
	{
		if (this.registries.TryGetValue(type.Location, out IRegistry? value))
		{
			registry = (IRegistry<T>)value;
			return true;
		}

		registry = null;
		return false;
	}
}
