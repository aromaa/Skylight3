using System.Text.Json;
using Skylight.API.Game.Clients;

namespace Skylight.API.Game.Purse;

public interface ICurrency
{
	public ICurrencyType Type { get; }
	public JsonDocument? Data { get; }

	public void Update(IClient client, int amount);
}
