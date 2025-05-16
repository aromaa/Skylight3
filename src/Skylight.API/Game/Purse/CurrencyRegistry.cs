using System.Collections.Concurrent;

namespace Skylight.API.Game.Purse;

public static class CurrencyRegistry
{
	private static readonly ConcurrentDictionary<string, byte> keys = new();

	static CurrencyRegistry()
	{
		CurrencyRegistry.Register(CurrencyKeys.Default);
		CurrencyRegistry.Register(CurrencyKeys.Silver);
	}

	public static IReadOnlyCollection<string> RegisteredKeys
		=> CurrencyRegistry.keys.Keys.ToArray();

	private static bool Register(string currencyKey)
		=> CurrencyRegistry.keys.TryAdd(currencyKey, 0);

	public static bool Unregister(string currencyKey)
		=> CurrencyRegistry.keys.TryRemove(currencyKey, out _);
}
