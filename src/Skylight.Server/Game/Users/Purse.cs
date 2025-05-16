using System.Collections.Concurrent;
using Skylight.API.Game.Purse;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Users;

internal sealed class Purse : IPurse
{
	private readonly ConcurrentDictionary<string, int> currencies;

	private Purse(Dictionary<string, int> initialCurrencies)
	{
		this.currencies = new ConcurrentDictionary<string, int>(initialCurrencies);
	}

	public static Purse FromDatabase(int userId, SkylightContext db, CancellationToken ct)
	{
		Dictionary<string, int> dbDict = db.UserPurse
			.Where(c => c.UserId == userId)
			.ToDictionary(c => c.Currency, c => c.Balance);

		Dictionary<string, int> merged = CurrencyRegistry.RegisteredKeys
			.ToDictionary(k => k, k => dbDict.GetValueOrDefault(k, 0));

		return new Purse(merged);
	}

	public int GetBalance(string currencyKey)
		=> this.currencies.GetValueOrDefault(currencyKey, 0);

	public void UpdateBalance(string currencyKey, int newBalance)
		=> this.currencies.AddOrUpdate(currencyKey, newBalance, (_, __) => newBalance);
}
