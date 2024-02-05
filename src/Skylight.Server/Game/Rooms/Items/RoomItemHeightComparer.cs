using Skylight.API.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items;

internal sealed class RoomItemHeightComparer : IComparer<IFloorRoomItem>
{
	public static readonly RoomItemHeightComparer Instance = new();

	public int Compare(IFloorRoomItem? x, IFloorRoomItem? y)
	{
		//Item with highest Z
		int result = x!.Position.Z.CompareTo(y!.Position.Z);
		if (result != 0)
		{
			return result;
		}

		//Item with highest height
		result = x.Height.CompareTo(y.Height);
		if (result != 0)
		{
			return result;
		}

		//Last resort, the most recently purchased item
		return x.Id.CompareTo(y.Id);
	}
}
