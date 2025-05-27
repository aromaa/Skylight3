using Skylight.API.Registry;

namespace Skylight.API.Game.Purse;

public interface IPurse
{
	int GetBalance(ResourceKey currency);
	void SetBalance(ResourceKey currency, int newBalance);
}
