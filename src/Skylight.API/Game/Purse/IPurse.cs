namespace Skylight.API.Game.Purse;

public interface IPurse
{
	int GetBalance(string currencyKey);
	void UpdateBalance(string currencyKey, int newBalance);
}
