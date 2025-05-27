using System.Data.Common;
using System.Text.Json;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.API.Registry;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransaction : IAsyncDisposable, IDisposable
{
	public DbTransaction Transaction { get; }
	public IUserInfo User { get; }

	public string ExtraData { get; }

	public void AddBadge(IBadge badge);

	public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData);
	public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData);

	int GetCurrencyBalance(ICurrency currency);
	void AddCurrency(ICurrency currency, int amount);
	void DeductCurrency(ICurrency currency, int amount);

	public Task CompleteAsync(CancellationToken cancellationToken = default);
}
