using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections.Immutable;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Effects;

internal sealed class WiredEffectInteractionHandler : IWiredEffectInteractionHandler
{
	private readonly IRoom room;

	private readonly ImmutableArray2D<WiredRoomTile> tiles;

	internal WiredEffectInteractionHandler(IRoom room)
	{
		this.room = room;

		ImmutableArray2D<WiredRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<WiredRoomTile>(room.Map.Layout.Size.X, room.Map.Layout.Size.Y);
		for (int x = 0; x < room.Map.Layout.Size.X; x++)
		{
			for (int y = 0; y < room.Map.Layout.Size.Y; y++)
			{
				builder[x, y] = new WiredRoomTile();
			}
		}

		this.tiles = builder.MoveToImmutable();
	}

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	public void OnPlace(IWiredEffectRoomItem effect)
	{
		WiredRoomTile tile = this.tiles[effect.Position.X, effect.Position.Y];
		tile.Add(effect);
	}

	public void OnMove(IWiredEffectRoomItem effect, Point3D newPosition)
	{
		WiredRoomTile oldTile = this.tiles[effect.Position.X, effect.Position.Y];
		WiredRoomTile newTile = this.tiles[newPosition.X, newPosition.Y];

		oldTile.Remove(effect);
		newTile.Add(effect);
	}

	public void OnRemove(IWiredEffectRoomItem effect)
	{
		WiredRoomTile tile = this.tiles[effect.Position.X, effect.Position.Y];
		tile.Remove(effect);
	}

	public void TriggerStack(IWiredRoomItem wired, IUserRoomUnit? cause = null)
	{
		WiredRoomTile tile = this.tiles[wired.Position.X, wired.Position.Y];
		tile.Trigger(cause);
	}

	private sealed class WiredRoomTile
	{
		private readonly SortedSet<IWiredEffectRoomItem> effects;

		internal WiredRoomTile()
		{
			this.effects = new SortedSet<IWiredEffectRoomItem>(RoomItemHeightComparer.Instance);
		}

		public void Add(IWiredEffectRoomItem effect)
		{
			this.effects.Add(effect);
		}

		public void Remove(IWiredEffectRoomItem effect)
		{
			this.effects.Remove(effect);
		}

		public void Trigger(IUserRoomUnit? cause)
		{
			foreach (IWiredEffectRoomItem effect in this.effects)
			{
				effect.Trigger(cause);
			}
		}
	}
}
