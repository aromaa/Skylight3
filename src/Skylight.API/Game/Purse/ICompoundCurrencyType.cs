using System.Text.Json;

namespace Skylight.API.Game.Purse;

public interface ICompoundCurrencyType<out T> : ICurrencyType
	where T : ICurrency
{
	public new T Create(JsonDocument? data = null);

	ICurrency ICurrencyType.Create(JsonDocument? data) => this.Create(data);
}
