namespace Skylight.API.Game.Furniture;

public interface IMultiStateFurniture : IStatefulFurniture
{
	public int StateCount { get; }
}
