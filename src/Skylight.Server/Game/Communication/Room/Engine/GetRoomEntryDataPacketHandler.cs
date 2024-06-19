using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Layout;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class GetRoomEntryDataPacketHandler<T> : UserPacketHandler<T>
	where T : IGetRoomEntryDataIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession is not { } roomSession)
		{
			return;
		}

		if (!roomSession.TryChangeState(IRoomSession.SessionState.EnterRoom, IRoomSession.SessionState.Ready))
		{
			return;
		}

		roomSession.Room!.PostTask(room =>
		{
			if (!roomSession.TryChangeState(IRoomSession.SessionState.InRoom, IRoomSession.SessionState.EnterRoom))
			{
				return;
			}

			room.Enter(roomSession.User);

			roomSession.User.SendAsync(new RoomEntryTileOutgoingPacket(room.Map.Layout.DoorLocation.X, room.Map.Layout.DoorLocation.Y, room.Map.Layout.DoorDirection));
			roomSession.User.SendAsync(new HeightMapOutgoingPacket
			{
				Width = room.Map.Layout.Size.X,
				HeightMap = Enumerable.Repeat((short)0, room.Map.Layout.Size.X * room.Map.Layout.Size.Y).ToArray()
			});
			roomSession.User.SendAsync(new FloorHeightMapOutgoingPacket
			{
				Scale = false,
				FixedWallsHeight = -1,
				HeightMap = room.Map.Layout.HeightMap
			});

			List<PublicRoomObjectData> publicRoomObjects = [];
			//TODO: Public items

			List<ObjectData> objects = [];
			foreach (IFloorRoomItem roomItem in room.ItemManager.FloorItems)
			{
				objects.Add(new ObjectData(roomItem.Id, roomItem.Furniture.Id, roomItem.Position.X, roomItem.Position.Y, roomItem.Position.Z, roomItem.Direction, roomItem.Height, 0, roomItem.GetItemData()));
			}

			List<ItemData> items = [];
			foreach (IWallRoomItem roomItem in room.ItemManager.WallItems)
			{
				items.Add(new ItemData(roomItem.Id, roomItem.Furniture.Id, new WallPosition(roomItem.Location.X, roomItem.Location.Y, roomItem.Position.X, roomItem.Position.Y), roomItem.GetItemData()));
			}

			roomSession.User.SendAsync(new PublicRoomObjectsOutgoingPacket(publicRoomObjects));
			roomSession.User.SendAsync(new ObjectsOutgoingPacket(objects, Array.Empty<(int, string)>()));
			roomSession.User.SendAsync(new ItemsOutgoingPacket(items, Array.Empty<(int, string)>()));

			List<RoomUnitData> units = [];

			foreach (IUserRoomUnit unit in room.UnitManager.Units)
			{
				units.Add(new RoomUnitData
				{
					IdentifierId = unit.User.Profile.Id,
					Name = unit.User.Profile.Username,
					Motto = unit.User.Profile.Motto,
					Figure = unit.User.Profile.Figure,
					RoomUnitId = unit.Id,
					X = unit.Position.X,
					Y = unit.Position.Y,
					Z = unit.Position.Z,
					Direction = unit.Rotation.X,
					Type = 1,
					Gender = unit.User.Profile.Gender,
					GroupId = 0,
					GroupStatus = 0,
					GroupName = string.Empty,
					SwimSuit = string.Empty,
					AchievementScore = 666,
					IsModerator = true
				});
			}

			roomSession.User.SendAsync(new UsersOutgoingPacket(units));

			roomSession.User.SendAsync(new RoomVisualizationSettingsOutgoingPacket(false, 0, 0));

			roomSession.User.SendAsync(new RoomEntryInfoOutgoingPacket(room.Info.Id, true));

			roomSession.EnterRoom(room.UnitManager.CreateUnit(roomSession.User));
		});
	}
}
