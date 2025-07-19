namespace Skylight.API.Game.Purse;

public interface IPurse
{
	public IEnumerable<(T Currency, int Balance)> GetBalances<T>(ICompoundCurrencyType<T> currencyType)
		where T : ICurrency;

	public int GetBalance(ICurrency currency);
	public void SetBalance(ICurrency currency, int newBalance);
}
