using System.Text.Json;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Purse;

namespace Skylight.Server.Game.Purse;

internal sealed class SimpleCurrencyType : ISimpleCurrencyType
{
	public ICurrency Value { get; }

	internal SimpleCurrencyType(Action<IClient, int> update)
	{
		this.Value = new Impl(this, update);
	}

	public ICurrency Create(JsonDocument? data = null)
	{
		if (data is not null)
		{
			throw new ArgumentException("This type does not support data", nameof(data));
		}

		return this.Value;
	}

	private sealed class Impl(ICurrencyType type, Action<IClient, int> update) : ICurrency
	{
		public ICurrencyType Type { get; } = type;
		public JsonDocument? Data => null;

		public void Update(IClient client, int amount) => update(client, amount);
	}
}
