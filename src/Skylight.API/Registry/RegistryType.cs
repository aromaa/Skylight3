namespace Skylight.API.Registry;

public readonly record struct RegistryType<T>(ResourceLocation Location)
{
	public RegistryType(ResourceKey container, ResourceKey resource)
		: this(new ResourceLocation(container, resource))
	{
	}
}
