using System.Data.Common;
using System.Text.Json;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransactionContext
{
	public IUserInfo User { get; }

	public string ExtraData { get; }

	public IConstraints Constraints { get; }
	public ICommands Commands { get; }

	public void AddConstraint(Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> action);
	public void AddCommand(Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> action);

	public interface IConstraints
	{
		public void DeductCurrency(ICurrency currency, int amount);
	}

	public interface ICommands
	{
		public void AddBadge(IBadge badge);

		public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData);
		public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData);
	}
}
