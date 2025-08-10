using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Game.Navigator;
using Skylight.Server.Game.Rooms.Items;
using Skylight.Server.Game.Rooms.Map.Private;
using Skylight.Server.Game.Rooms.Units.Private;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoom : Room, IPrivateRoom
{
	public override IPrivateRoomInfo Info { get; }
	public override IPrivateRoomMap Map { get; }
	public override IRoomUnitManager UnitManager { get; }

	public IRoomItemManager ItemManager { get; }

	private readonly double[,] tileHeights;

	public PrivateRoom(IPrivateRoomInfo info, IRoomLayout roomLayout, IRegistryHolder registryHolder, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IUserManager userManager, IRoomItemInteractionManager itemInteractionManager, RoomActivityWorker roomActivityWorker)
		: base(roomLayout)
	{
		this.Info = info;
		this.Map = new PrivateRoomMap(this, roomLayout, registryHolder);

		this.ItemManager = new RoomItemManager(this, roomLayout, registryHolder, dbContextFactory, userManager, furnitureManager, floorRoomItemStrategy, wallRoomItemStrategy, itemInteractionManager);

		this.UnitManager = new PrivateRoomUnitManager(this, roomActivityWorker);

		this.tileHeights = new double[roomLayout.Size.X, roomLayout.Size.Y];

		for (int x = 0; x < this.Map.Layout.Size.X; x++)
		{
			for (int y = 0; y < this.Map.Layout.Size.Y; y++)
			{
				this.tileHeights[x, y] = this.Map.GetTile(x, y).Position.Z;
			}
		}
	}

	public override async Task LoadAsync(CancellationToken cancellationToken)
	{
		await this.ItemManager.LoadAsync(cancellationToken).ConfigureAwait(false);
	}

	internal override void DoTick()
	{
		this.ItemManager.Tick();

		List<(int X, int Y, TileHeightMap Data)>? heightMapUpdates = null;
		for (int x = 0; x < this.Map.Layout.Size.X; x++)
		{
			for (int y = 0; y < this.Map.Layout.Size.Y; y++)
			{
				double z = this.Map.GetTile(x, y).Position.Z;
				if (this.tileHeights[x, y] != z)
				{
					this.tileHeights[x, y] = z;

					//TODO: This is an impl detail, just no good way to represent that atm.
					if (heightMapUpdates is { Count: >= 127 })
					{
						this.SendAsync(new HeightMapUpdateOutgoingPacket(heightMapUpdates));

						heightMapUpdates = null;
					}

					heightMapUpdates ??= [];
					heightMapUpdates.Add((x, y, new TileHeightMap(z, false, true)));
				}
			}
		}

		if (heightMapUpdates is not null)
		{
			this.SendAsync(new HeightMapUpdateOutgoingPacket(heightMapUpdates));
		}
	}

	public bool IsOwner(IUser user) => this.Info.Owner.Id == user.Id;
}
