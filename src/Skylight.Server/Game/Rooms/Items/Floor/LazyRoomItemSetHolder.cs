using System.Runtime.CompilerServices;
using Skylight.API.Game.Rooms.Items;

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
	public HashSet<IRoomItem> Get(IRoomItemManager itemManager)
	{
		object list = this.list;

		return list.GetType() != typeof(HashSet<IRoomItem>)
			? this.Create(itemManager)
			: Unsafe.As<HashSet<IRoomItem>>(list);
	}

	private HashSet<IRoomItem> Create(IRoomItemManager itemManager)
	{
		HashSet<int> stripIds = (HashSet<int>)this.list;

		HashSet<IRoomItem> items = new(stripIds.Count);
		foreach (int stripId in stripIds)
		{
			if (itemManager.TryGetItem(stripId, out IRoomItem? item))
			{
				items.Add(item);
			}
		}

		this.list = items;

		return items;
	}
}
