namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticManager : IFurniMaticSnapshot
{
	public IFurniMaticSnapshot Current { get; }

	public Task LoadAsync(CancellationToken cancellationToken = default);
}
