using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Rooms.Items;

internal sealed class RoomItemManager : IRoomItemManager
{
	private readonly Room room;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFurnitureManager furnitureManager;

	private readonly IFloorRoomItemStrategy floorRoomItemStrategy;
	private readonly IWallRoomItemStrategy wallRoomItemStrategy;
	private readonly IRoomItemInteractionManager itemInteractionManager;

	private readonly IUserManager userManager;

	private readonly Dictionary<int, IFloorRoomItem> floorItems;
	private readonly Dictionary<int, IWallRoomItem> wallItems;

	private readonly Dictionary<Type, IRoomItemInteractionHandler> interactionHandlers;

	internal RoomItemManager(Room room, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IRoomItemInteractionManager itemInteractionManager, IUserManager userManager)
	{
		this.room = room;

		this.dbContextFactory = dbContextFactory;

		this.furnitureManager = furnitureManager;

		this.floorRoomItemStrategy = floorRoomItemStrategy;
		this.wallRoomItemStrategy = wallRoomItemStrategy;
		this.itemInteractionManager = itemInteractionManager;

		this.userManager = userManager;

		this.floorItems = new Dictionary<int, IFloorRoomItem>();
		this.wallItems = new Dictionary<int, IWallRoomItem>();

		this.interactionHandlers = this.itemInteractionManager.CreateHandlers(this.room);
	}

	public IEnumerable<IFloorRoomItem> FloorItems => this.floorItems.Values;
	public IEnumerable<IWallRoomItem> WallItems => this.wallItems.Values;

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		IFurnitureSnapshot furnitures = this.furnitureManager.Current;

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		await foreach (FloorItemEntity floorItem in dbContext.FloorItems
					 .AsNoTracking()
					 .Where(i => i.RoomId == this.room.Info.Id)
					 .AsAsyncEnumerable()
					 .WithCancellation(cancellationToken))
		{
			if (!furnitures.TryGetFloorFurniture(floorItem.FurnitureId, out IFloorFurniture? furniture))
			{
				throw new Exception($"Floor furniture {floorItem.FurnitureId} not found");
			}

			IUserInfo? owner = await this.userManager.GetUserInfoAsync(floorItem.UserId, cancellationToken).ConfigureAwait(false);
			if (owner is null)
			{
				throw new Exception($"User {floorItem.UserId} not found");
			}

			IFloorRoomItem item = this.floorRoomItemStrategy.CreateFloorItem(this.room, floorItem.Id, owner, furniture, new Point3D(floorItem.X, floorItem.Y, floorItem.Z), floorItem.Direction, floorItem.ExtraData);

			this.AddItem(item);
		}

		await foreach (WallItemEntity wallItem in dbContext.WallItems
						  .AsNoTracking()
						  .Where(i => i.RoomId == this.room.Info.Id)
						  .AsAsyncEnumerable()
						  .WithCancellation(cancellationToken))
		{
			if (!furnitures.TryGetWallFurniture(wallItem.FurnitureId, out IWallFurniture? furniture))
			{
				throw new Exception($"Wall furniture {wallItem.FurnitureId} not found");
			}

			IUserInfo? owner = await this.userManager.GetUserInfoAsync(wallItem.UserId, cancellationToken).ConfigureAwait(false);
			if (owner is null)
			{
				throw new Exception($"User {wallItem.UserId} not found");
			}

			IWallRoomItem item = this.wallRoomItemStrategy.CreateWallItem(this.room, wallItem.Id, owner, furniture, new Point2D(wallItem.LocationX, wallItem.LocationY), new Point2D(wallItem.PositionX, wallItem.PositionY), wallItem.ExtraData);

			this.AddItem(item);
		}
	}

	public void AddItem(IFloorRoomItem item)
	{
		this.floorItems.Add(item.Id, item);

		this.AddItemToTile(item);

		item.OnPlace();

		this.room.SendAsync(new ObjectAddOutgoingPacket(new ObjectData(item.Id, item.Furniture.Id, item.Position.X, item.Position.Y, item.Position.Z, 0, 0, 0, item.GetItemData()), item.Owner.Username));
	}

	public void AddItem(IWallRoomItem item)
	{
		this.wallItems.Add(item.Id, item);

		this.AddItemToTile(item);

		item.OnPlace();

		this.room.SendAsync(new ItemAddOutgoingPacket(new ItemData(item.Id, item.Furniture.Id, new WallPosition(item.Location.X, item.Location.Y, item.Position.X, item.Position.Y), item.GetItemData()), item.Owner.Username));
	}

	public bool CanPlaceItem(IFloorFurniture furniture, Point3D position, IUser? user)
	{
		if (this.itemInteractionManager.TryGetHandler(furniture, out Type? handlerType)
			&& this.interactionHandlers.TryGetValue(handlerType, out IRoomItemInteractionHandler? value)
			&& !value.CanPlaceItem(furniture, position.XY))
		{
			return false;
		}

		foreach (Point2D point in furniture.EffectiveTiles)
		{
			if (!this.room.Map.IsValidLocation(position.XY + point))
			{
				return false;
			}

			IRoomTile tile = this.room.Map.GetTile(position.XY + point);
			if (tile.IsHole || tile.HasRoomUnit)
			{
				return false;
			}
		}

		return true;
	}

	public bool CanPlaceItem(IWallFurniture furniture, Point2D location, Point2D position, IUser? user)
	{
		if (this.itemInteractionManager.TryGetHandler(furniture, out Type? handlerType)
			&& this.interactionHandlers.TryGetValue(handlerType, out IRoomItemInteractionHandler? value)
			&& !value.CanPlaceItem(furniture, location))
		{
			return false;
		}

		return true;
	}

	public double GetPlacementHeight(IFloorFurniture item, Point2D location)
	{
		double z = 0;

		foreach (Point2D point in item.EffectiveTiles)
		{
			IRoomTile tile = this.room.Map.GetTile(location + point);

			double tileZ = tile.Position.Z;
			if (tileZ > z)
			{
				z = tileZ;
			}
		}

		return z;
	}

	private void AddItemToTile(IFloorRoomItem item)
	{
		Point2D location = item.Position.XY;

		foreach (Point2D point in item.Furniture.EffectiveTiles)
		{
			this.room.Map.GetTile(location + point).AddItem(item);
		}
	}

	private void AddItemToTile(IWallRoomItem item)
	{
	}

	public void MoveItem(IFloorRoomItem item, Point2D location, int direction)
	{
		this.RemoveItemFromTile(item);

		Point3D position = new(location, this.GetPlacementHeight(item.Furniture, location));

		this.MoveItemInternal(item, position, direction);

		this.room.SendAsync(new ObjectUpdateOutgoingPacket(new ObjectData(item.Id, item.Furniture.Id, position.X, position.Y, position.Z, direction, 0, 0, item.GetItemData())));
	}

	public void MoveItem(IFloorRoomItem item, Point3D position, int direction)
	{
		this.RemoveItemFromTile(item);
		this.MoveItemInternal(item, position, direction);

		this.room.SendAsync(new ObjectUpdateOutgoingPacket(new ObjectData(item.Id, item.Furniture.Id, position.X, position.Y, position.Z, direction, 0, 0, item.GetItemData())));
	}

	private void MoveItemInternal(IFloorRoomItem item, Point3D position, int direction)
	{
		item.OnMove(position, direction);

		this.AddItemToTile(item);
	}

	public void RemoveItem(IFloorRoomItem item)
	{
		if (!this.floorItems.Remove(item.Id))
		{
			return;
		}

		this.RemoveItemFromTile(item);

		item.OnRemove();

		this.room.SendAsync(new ObjectRemoveOutgoingPacket(item.Id, false, 0, 0));
	}

	public void RemoveItem(IWallRoomItem item)
	{
		if (!this.wallItems.Remove(item.Id))
		{
			return;
		}

		this.RemoveItemFromTile(item);

		item.OnRemove();

		this.room.SendAsync(new ItemRemoveOutgoingPacket(item.Id, 0));
	}

	private void RemoveItemFromTile(IFloorRoomItem item)
	{
		Point2D location = item.Position.XY;

		foreach (Point2D point in item.Furniture.EffectiveTiles)
		{
			this.room.Map.GetTile(location + point).RemoveItem(item);
		}
	}

	private void RemoveItemFromTile(IWallRoomItem item)
	{
	}

	public bool TryGetFloorItem(int id, [NotNullWhen(true)] out IFloorRoomItem? item) => this.floorItems.TryGetValue(id, out item);
	public bool TryGetWallItem(int id, [NotNullWhen(true)] out IWallRoomItem? item) => this.wallItems.TryGetValue(id, out item);

	public bool TryGetInteractionHandler<T>([NotNullWhen(true)] out T? handler)
		where T : IRoomItemInteractionHandler
	{
		Unsafe.SkipInit(out handler);

		return this.interactionHandlers.TryGetValue(typeof(T), out Unsafe.As<T?, IRoomItemInteractionHandler?>(ref handler));
	}
}
