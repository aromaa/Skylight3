using System.Text.Json;

namespace Skylight.API.Game.Purse;

public interface ICurrencyType
{
	public ICurrency Create(JsonDocument? data = null);
}
