namespace Skylight.API.Game.Furniture;

public interface IFurnitureManager : IFurnitureSnapshot
{
	public IFurnitureSnapshot Current { get; }

	public Task<IFurnitureSnapshot> LoadAsync(CancellationToken cancellationToken = default);
}
