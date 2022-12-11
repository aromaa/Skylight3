using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class FloorRoomItemStrategy : IFloorRoomItemStrategy
{
	private readonly IFloorRoomItemFactory[] factories;

	public FloorRoomItemStrategy(IEnumerable<IFloorRoomItemFactory> factories)
	{
		this.factories = factories.ToArray();
	}

	public TRoomItem CreateFloorItem<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, TData data)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
	{
		foreach (IFloorRoomItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TRoomItem, TData>(room, itemId, owner, furniture, position, direction, data);
			}
		}

		throw new NotSupportedException();
	}

	public TRoomItem CreateFloorItem<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	{
		foreach (IFloorRoomItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TRoomItem>(room, itemId, owner, furniture, position, direction, extraData);
			}
		}

		throw new NotSupportedException();
	}
}
