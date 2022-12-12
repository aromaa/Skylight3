using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class SoundMachineRoomItemFactory : FloorRoomItemFactory<ISoundMachineFurniture, SoundMachineRoomItem>
{
	public override SoundMachineRoomItem Create(IRoom room, int itemId, IUserInfo owner, ISoundMachineFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		if (!room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler))
		{
			throw new Exception("SoundMachineInteractionManager not found");
		}

		return new SoundMachineRoomItem(room, itemId, owner, furniture, position, direction, handler);
	}
}
