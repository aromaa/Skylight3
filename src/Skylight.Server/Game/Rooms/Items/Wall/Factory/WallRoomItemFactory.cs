using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall.Factory;

internal abstract class WallRoomItemFactory<TFurniture, TRoomItem, TData> : IWallRoomItemFactory
	where TFurniture : IWallFurniture
	where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
{
	public bool Supports(IWallFurniture furniture) => furniture is TFurniture;

	public TRoomItem1 Create<TFurniture1, TRoomItem1, TData1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point2D location, Point2D position, TData1 data)
		where TFurniture1 : IWallFurniture
		where TRoomItem1 : IWallRoomItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, location, position, (TData)(object)data!);
	}

	public TRoomItem1 Create<TFurniture1, TRoomItem1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point2D location, Point2D position, JsonDocument? extraData)
		where TFurniture1 : IWallFurniture
		where TRoomItem1 : IWallRoomItem, IFurnitureItem<TFurniture1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, location, position, extraData);
	}

	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, TData data);
	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData);
}

internal abstract class WallRoomItemFactory<TFurniture, TRoomItem> : IWallRoomItemFactory
	where TFurniture : IWallFurniture
	where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
{
	public bool Supports(IWallFurniture furniture) => furniture is TFurniture;

	public TRoomItem1 Create<TFurniture1, TRoomItem1, TData1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point2D location, Point2D position, TData1 data)
		where TFurniture1 : IWallFurniture
		where TRoomItem1 : IWallRoomItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, location, position, null);
	}

	public TRoomItem1 Create<TFurniture1, TRoomItem1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point2D location, Point2D position, JsonDocument? extraData)
		where TFurniture1 : IWallFurniture
		where TRoomItem1 : IWallRoomItem, IFurnitureItem<TFurniture1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, location, position, extraData);
	}

	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData);
}
