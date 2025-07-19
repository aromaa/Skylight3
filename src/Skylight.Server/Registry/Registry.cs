using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Skylight.API;
using Skylight.API.Registry;

namespace Skylight.Server.Registry;

internal sealed class Registry<T>(RegistryType<T> type, FrozenDictionary<ResourceKey, T> values) : IRegistry<T>
	where T : notnull
{
	private readonly FrozenDictionary<ResourceKey, T> values = values;
	private readonly FrozenDictionary<T, ResourceKey> keys = values.ToFrozenDictionary(e => e.Value, e => e.Key);

	public RegistryType<T> Type { get; } = type;
	public IEnumerable<T> Values => this.values.Values;

	public T Value(ResourceKey key) => this.values[key];
	public bool TryGetValue(ResourceKey key, [NotNullWhen(true)] out T? value) => this.values.TryGetValue(key, out value);

	public ResourceKey Key(T value) => this.keys[value];

	internal static Registry<T> Create(RegistryType<T> type, params IEnumerable<(ResourceKey Key, T Value)> values)
		=> new(type, values.ToFrozenDictionary(v => v.Key, v => v.Value));
}
