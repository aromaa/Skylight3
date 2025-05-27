using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Skylight.API;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Users;

internal sealed class Purse : IPurse
{
	private readonly ConcurrentDictionary<ResourceKey, int> balances;

	private Purse(IDictionary<ResourceKey, int> snapshot)
	{
		this.balances = new ConcurrentDictionary<ResourceKey, int>(snapshot);
	}

	public static Purse FromDatabase(
		int userId,
		SkylightContext db,
		CancellationToken ct = default)
	{
		Dictionary<ResourceKey, int> snapshot = db.UserPurse
			.Where(p => p.UserId == userId)
			.AsNoTracking()
			.ToDictionary(
				p => ResourceKey.Parse(p.Currency),
				p => p.Balance);
		return new Purse(snapshot);
	}

	public int GetBalance(ResourceKey currency) => this.balances.GetValueOrDefault(currency, 0);

	public void SetBalance(ResourceKey currency, int newBalance) => this.balances.AddOrUpdate(currency, newBalance, (_, _) => newBalance);
}
