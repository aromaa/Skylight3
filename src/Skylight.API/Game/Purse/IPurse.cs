using Skylight.API.Registry;

namespace Skylight.API.Game.Purse;

public interface IPurse
{
	int GetBalance(RegistryReference<Currency> currency);
	void UpdateBalance(RegistryReference<Currency> currency, int newBalance);
}
