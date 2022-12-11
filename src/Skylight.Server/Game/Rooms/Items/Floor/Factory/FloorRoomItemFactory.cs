using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal abstract class FloorRoomItemFactory<TFurniture, TRoomItem, TData> : IFloorRoomItemFactory
	where TFurniture : IFloorFurniture
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
{
	public bool Supports(IFloorFurniture furniture) => furniture is TFurniture;

	public TRoomItem1 Create<TFurniture1, TRoomItem1, TData1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point3D position, int direction, TData1 data)
		where TFurniture1 : IFloorFurniture
		where TRoomItem1 : IFloorRoomItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, position, direction, (TData)(object)data!);
	}

	public TRoomItem1 Create<TFurniture1, TRoomItem1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point3D position, int direction, JsonDocument? extraData)
		where TFurniture1 : IFloorFurniture
		where TRoomItem1 : IFloorRoomItem, IFurnitureItem<TFurniture1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, position, direction, extraData);
	}

	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, TData data);
	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData);
}

internal abstract class FloorRoomItemFactory<TFurniture, TRoomItem> : IFloorRoomItemFactory
	where TFurniture : IFloorFurniture
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
{
	public bool Supports(IFloorFurniture furniture) => furniture is TFurniture;

	public TRoomItem1 Create<TFurniture1, TRoomItem1, TData1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point3D position, int direction, TData1 data)
		where TFurniture1 : IFloorFurniture
		where TRoomItem1 : IFloorRoomItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, position, direction, null);
	}

	public TRoomItem1 Create<TFurniture1, TRoomItem1>(IRoom room, int itemId, IUserInfo owner, TFurniture1 furniture, Point3D position, int direction, JsonDocument? extraData)
		where TFurniture1 : IFloorFurniture
		where TRoomItem1 : IFloorRoomItem, IFurnitureItem<TFurniture1>
	{
		return (TRoomItem1)(object)this.Create(room, itemId, owner, (TFurniture)(object)furniture, position, direction, extraData);
	}

	public abstract TRoomItem Create(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData);
}
