using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Skylight.API;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Users;

internal sealed class Purse : IPurse
{
	private readonly IClient client;

	private readonly ConcurrentDictionary<ICurrency, int> balances;

	private Purse(IClient client, Dictionary<ICurrency, int> snapshot)
	{
		this.client = client;

		this.balances = new ConcurrentDictionary<ICurrency, int>(snapshot);
	}

	public static async Task<Purse> FromDatabaseAsync(IClient client, IRegistry<ICurrencyType> currencyRegistry, SkylightContext db, int userId, CancellationToken cancellationToken = default)
	{
		Dictionary<ICurrency, int> purse = await db.UserPurse.Where(e => e.UserId == userId)
			.ToDictionaryAsync(a => currencyRegistry.Value(ResourceKey.Parse(a.CurrencyType))
				.Create(a.CurrencyData is { } currencyData ? JsonDocument.Parse(currencyData) : null), a => a.Balance, cancellationToken)
			.ConfigureAwait(false);

		return new Purse(client, purse);
	}

	public IEnumerable<(T Currency, int Balance)> GetBalances<T>(ICompoundCurrencyType<T> currencyType)
		where T : ICurrency => this.balances.Where(c => c.Key.Type == currencyType).Select(c => ((T)c.Key, c.Value));

	public int GetBalance(ICurrency currency) => this.balances.GetValueOrDefault(currency, 0);

	public void SetBalance(ICurrency currency, int newBalance)
	{
		this.balances.AddOrUpdate(currency, newBalance, (_, _) => newBalance);

		currency.Update(this.client, newBalance);
	}
}
