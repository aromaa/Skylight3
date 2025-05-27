using Skylight.API.Registry;

namespace Skylight.API.Game.Purse;

public static class Currencies
{
	private static RegistryReference<Currency> Ref(string path) =>
		new(RegistryTypes.Currency, ResourceKey.Parse(path));

	public static readonly RegistryReference<Currency> Credits = Currencies.Ref("skylight:credits");
	public static readonly RegistryReference<Currency> Silver = Currencies.Ref("skylight:silver");
}
