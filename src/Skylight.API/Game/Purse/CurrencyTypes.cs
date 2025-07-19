using Skylight.API.Registry;

namespace Skylight.API.Game.Purse;

public static class CurrencyTypes
{
	public static readonly RegistryReference<ICompoundCurrencyType<IActivityPointsCurrency>, ICurrencyType> ActivityPoints = CurrencyTypes.Reference<ICompoundCurrencyType<IActivityPointsCurrency>>("activity_points");

	public static readonly RegistryReference<ISimpleCurrencyType, ICurrencyType> Credits = CurrencyTypes.Reference<ISimpleCurrencyType>("credits");

	public static readonly RegistryReference<ISimpleCurrencyType, ICurrencyType> Silver = CurrencyTypes.Reference<ISimpleCurrencyType>("silver");

	private static RegistryReference<T, ICurrencyType> Reference<T>(string value)
		where T : ICurrencyType => new(RegistryTypes.Currency, ResourceKey.Skylight(value));
}
