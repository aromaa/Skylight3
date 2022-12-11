using System.Data.Common;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransaction : IAsyncDisposable, IDisposable
{
	public DbTransaction Transaction { get; }

	public string ExtraData { get; }

	public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData);
	public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData);

	public Task CompleteAsync(CancellationToken cancellationToken = default);
}
