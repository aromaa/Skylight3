using System.Runtime.CompilerServices;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal struct LazyRoomItemSetHolder
{
	private object list;

	internal LazyRoomItemSetHolder(HashSet<IRoomItem> items)
	{
		this.list = items;
	}

	internal LazyRoomItemSetHolder(HashSet<int> stripIds)
	{
		this.list = stripIds;
	}

	public void Set(HashSet<int> items) => this.list = items;
	public void Set(HashSet<IRoomItem> items) => this.list = items;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HashSet<IRoomItem> Get(IRoomItemManager itemManager, IRoomItemDomain normalRoomItemDomain)
	{
		object list = this.list;

		return list.GetType() != typeof(HashSet<IRoomItem>)
			? this.Create(itemManager, normalRoomItemDomain)
			: Unsafe.As<HashSet<IRoomItem>>(list);
	}

	private HashSet<IRoomItem> Create(IRoomItemManager itemManager, IRoomItemDomain normalRoomItemDomain)
	{
		HashSet<int> stripIds = (HashSet<int>)this.list;

		HashSet<IRoomItem> items = new(stripIds.Count);
		foreach (int stripId in stripIds)
		{
			if (itemManager.TryGetFloorItem(new RoomItemId(normalRoomItemDomain, stripId), out IFloorRoomItem? item))
			{
				items.Add(item);
			}
		}

		this.list = items;

		return items;
	}
}
