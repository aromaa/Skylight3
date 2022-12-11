using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticSnapshot
{
	public IFurniMaticPrizes Prizes { get; }

	public int ItemsRequiredToRecycle { get; }

	public Task<IFurniMaticPrize?> RecycleAsync(IUser user, IEnumerable<IFurnitureInventoryItem> items, CancellationToken cancellationToken = default);
	public Task<IFurniMaticPrize?> OpenGiftAsync(IUser user, IFurniMaticGiftRoomItem gift, CancellationToken cancellationToken = default);
}
