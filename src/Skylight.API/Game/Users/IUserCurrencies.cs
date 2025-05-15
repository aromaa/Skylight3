namespace Skylight.API.Game.Users;

public interface IUserCurrencies
{
	int GetBalance(string currencyKey);
	void UpdateBalance(string currencyKey, int newBalance);
}
