using System.Collections.Immutable;
using Net.Communication.Attributes;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Recycler;
using Skylight.Protocol.Packets.Data.Room.Object;
using Skylight.Protocol.Packets.Incoming.Recycler;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Recycler;

namespace Skylight.Server.Game.Communication.Recycler;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetRecyclerPrizesPacketHandler<T> : UserPacketHandler<T>
	where T : IGetRecyclerPrizesIncomingPacket
{
	private readonly IFurniMaticManager furniMaticManager;

	public GetRecyclerPrizesPacketHandler(IFurniMaticManager furniMaticManager)
	{
		this.furniMaticManager = furniMaticManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		List<RecyclerPrizeLevelData> rewards = new();

		ImmutableArray<IFurniMaticPrizeLevel> levels = this.furniMaticManager.Prizes.Levels;
		for (int i = levels.Length - 1; i >= 0; i--)
		{
			IFurniMaticPrizeLevel prizeLevel = levels[i];

			List<RecyclerPrizeData> prizes = new();

			foreach (IFurniMaticPrize prize in prizeLevel.Prizes)
			{
				List<RecyclerItemData> items = new();

				foreach (IFurniture item in prize.Furnitures)
				{
					items.Add(new RecyclerItemData(item is IFloorFurniture ? FurnitureType.Floor : FurnitureType.Wall, item.Id));
				}

				prizes.Add(new RecyclerPrizeData(prize.Name, items));
			}

			rewards.Add(new RecyclerPrizeLevelData(prizeLevel.Level, prizeLevel.Odds, prizes));
		}

		user.SendAsync(new RecyclerPrizesOutgoingPacket(rewards));
	}
}
