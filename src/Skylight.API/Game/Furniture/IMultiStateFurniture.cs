namespace Skylight.API.Game.Furniture;

public interface IMultiStateFurniture : IInteractableFurniture
{
	public int StateCount { get; }
}
