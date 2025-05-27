using Skylight.API.Registry;

namespace Skylight.API.Game.Purse;

public static class Currencies
{
	private static RegistryReference<ICurrency> Ref(string path) =>
		new(RegistryTypes.Currency, ResourceKey.Parse(path));

	public static readonly RegistryReference<ICurrency> Credits = Currencies.Ref("skylight:credits");
	public static readonly RegistryReference<ICurrency> Silver = Currencies.Ref("skylight:silver");
}
