using System.Buffers;
using System.Diagnostics;

namespace Skylight.API;

public readonly struct ResourceKey : IEquatable<ResourceKey>
{
	private static readonly SearchValues<char> validNamespaceChars = SearchValues.Create("_-0123456789abcdefghijklmnopqrstuvwxyz");
	private static readonly SearchValues<char> validValueChars = SearchValues.Create("_-/0123456789abcdefghijklmnopqrstuvwxyz");

	public const string SkylightNamespace = "skylight";

	public string Namespace { get; }
	public string Value { get; }

	private ResourceKey(string @namespace, string value)
	{
		Debug.Assert(ResourceKey.ValidNamespace(@namespace));
		Debug.Assert(ResourceKey.ValidValue(value));

		this.Namespace = @namespace;
		this.Value = value;
	}

	public bool Equals(ResourceKey other) => this.Namespace == other.Namespace && this.Value == other.Value;

	public override bool Equals(object? obj) => obj is ResourceKey other && this.Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.Namespace, this.Value);
	public override string ToString() => $"{this.Namespace}:{this.Value}";

	public static bool operator ==(ResourceKey left, ResourceKey right) => left.Equals(right);
	public static bool operator !=(ResourceKey left, ResourceKey right) => !left.Equals(right);

	public static ResourceKey Create(string @namespace, string value) => new(ResourceKey.ValidateNamespace(@namespace), ResourceKey.ValidateValue(value));
	public static ResourceKey Skylight(string value) => new(ResourceKey.SkylightNamespace, ResourceKey.ValidateValue(value));

	public static ResourceKey Parse(string value)
	{
		int index = value.IndexOf(':');

		return index == -1
			? ResourceKey.Skylight(value)
			: ResourceKey.Create(value[..index], value[(index + 1)..]);
	}

	private static bool ValidNamespace(string @namespace) => @namespace.AsSpan().IndexOfAnyExcept(ResourceKey.validNamespaceChars) == -1;
	private static bool ValidValue(string value) => value.AsSpan().IndexOfAnyExcept(ResourceKey.validValueChars) == -1;

	private static string ValidateNamespace(string @namespace)
	{
		if (ResourceKey.ValidNamespace(@namespace))
		{
			return @namespace;
		}

		throw new ArgumentException("Non [a-z0-9_-] character", nameof(@namespace));
	}

	private static string ValidateValue(string value)
	{
		if (ResourceKey.ValidValue(value))
		{
			return value;
		}

		throw new ArgumentException("Non [a-z0-9_-/] character", nameof(value));
	}
}
