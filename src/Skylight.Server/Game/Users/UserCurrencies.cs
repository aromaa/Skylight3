using System.Collections.Concurrent;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Users;

internal sealed class UserCurrencies(Dictionary<string, int> initialCurrencies) : IUserCurrencies
{
	private readonly ConcurrentDictionary<string, int> currencies = new(initialCurrencies);

	public static UserCurrencies FromDatabase(int userId, SkylightContext db, CancellationToken ct)
	{
		Dictionary<string, int> dbDict = db.UserCurrencies
			.Where(c => c.UserId == userId)
			.ToDictionary(c => c.Currency, c => c.Balance);

		Dictionary<string, int> merged = CurrencyKeys.All
			.ToDictionary(k => k, k => dbDict.GetValueOrDefault(k, 0));

		return new UserCurrencies(merged);
	}

	public int GetBalance(string currencyKey) => this.currencies.GetValueOrDefault(currencyKey, 0);

	public void UpdateBalance(string currencyKey, int newBalance) => this.currencies.AddOrUpdate(currencyKey, newBalance, (key, existing) => newBalance);
}
