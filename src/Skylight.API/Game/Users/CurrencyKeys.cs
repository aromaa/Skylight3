namespace Skylight.API.Game.Users;

public static class CurrencyKeys
{
	public const string Credits = "skylight:credits";
	public const string Silver = "skylight:silver";

	public static readonly IReadOnlyList<string> All = [CurrencyKeys.Credits, CurrencyKeys.Silver];
}
