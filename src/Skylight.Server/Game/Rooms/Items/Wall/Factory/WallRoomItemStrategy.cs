using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall.Factory;

internal sealed class WallRoomItemStrategy : IWallRoomItemStrategy
{
	private readonly IWallRoomItemFactory[] factories;

	public WallRoomItemStrategy(IEnumerable<IWallRoomItemFactory> factories)
	{
		this.factories = factories.ToArray();
	}

	public TRoomItem CreateWallItem<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, TData data)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
	{
		foreach (IWallRoomItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TRoomItem, TData>(room, itemId, owner, furniture, location, position, data);
			}
		}

		throw new NotSupportedException();
	}

	public TRoomItem CreateWallItem<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	{
		foreach (IWallRoomItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TRoomItem>(room, itemId, owner, furniture, location, position, extraData);
			}
		}

		throw new NotSupportedException();
	}
}
