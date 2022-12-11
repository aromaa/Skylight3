namespace Skylight.API.Game.Furniture;

public interface IFurnitureItem<out T>
	where T : IFurniture
{
	public T Furniture { get; }
}
