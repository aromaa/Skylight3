using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms;
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
internal sealed class GetRoomEntryDataPacketHandler<T> : UserPacketHandler<T>
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

		roomSession.Room!.ScheduleTask(new EnterRoomTask
		{
			Session = roomSession
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct EnterRoomTask : IRoomTask
	{
		internal IRoomSession Session { get; init; }

		public void Execute(IRoom room)
		{
			if (!this.Session.TryChangeState(IRoomSession.SessionState.InRoom, IRoomSession.SessionState.EnterRoom))
			{
				return;
			}

			room.Enter(this.Session.User);

			this.Session.User.SendAsync(new RoomEntryTileOutgoingPacket(room.Map.Layout.DoorLocation.X, room.Map.Layout.DoorLocation.Y, room.Map.Layout.DoorDirection));
			this.Session.User.SendAsync(new HeightMapOutgoingPacket
			{
				Width = room.Map.Layout.Size.X,
				HeightMap = Enumerable.Repeat((short)0, room.Map.Layout.Size.X * room.Map.Layout.Size.Y).ToArray()
			});
			this.Session.User.SendAsync(new FloorHeightMapOutgoingPacket
			{
				Scale = false,
				FixedWallsHeight = -1,
				HeightMap = room.Map.Layout.HeightMap
			});

			List<PublicRoomObjectData> publicRoomObjects = new();
			//TODO: Public items

			List<ObjectData> objects = new();
			foreach (IFloorRoomItem roomItem in room.ItemManager.FloorItems)
			{
				objects.Add(new ObjectData(roomItem.Id, roomItem.Furniture.Id, roomItem.Position.X, roomItem.Position.Y, roomItem.Position.Z, 0, 0, 0, roomItem.GetItemData()));
			}

			List<ItemData> items = new();
			foreach (IWallRoomItem roomItem in room.ItemManager.WallItems)
			{
				items.Add(new ItemData(roomItem.Id, roomItem.Furniture.Id, new WallPosition(roomItem.Location.X, roomItem.Location.Y, roomItem.Position.X, roomItem.Position.Y), roomItem.GetItemData()));
			}

			this.Session.User.SendAsync(new PublicRoomObjectsOutgoingPacket(publicRoomObjects));
			this.Session.User.SendAsync(new ObjectsOutgoingPacket(objects, Array.Empty<(int, string)>()));
			this.Session.User.SendAsync(new ItemsOutgoingPacket(items, Array.Empty<(int, string)>()));

			List<RoomUnitData> units = new();

			foreach (IUserRoomUnit unit in room.UnitManager.Units)
			{
				units.Add(new RoomUnitData
				{
					IdentifierId = unit.User.Profile.Id,
					Name = unit.User.Profile.Username,
					Motto = "Skylight",
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

			this.Session.User.SendAsync(new UsersOutgoingPacket(units));

			this.Session.User.SendAsync(new RoomVisualizationSettingsOutgoingPacket(false, 0, 0));

			this.Session.User.SendAsync(new RoomEntryInfoOutgoingPacket(room.Info.Id, true));

			this.Session.EnterRoom(room.UnitManager.CreateUnit(this.Session.User));
		}
	}
}
