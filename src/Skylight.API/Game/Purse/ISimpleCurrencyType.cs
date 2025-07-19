namespace Skylight.API.Game.Purse;

public interface ISimpleCurrencyType : ICurrencyType
{
	public ICurrency Value { get; }
}
