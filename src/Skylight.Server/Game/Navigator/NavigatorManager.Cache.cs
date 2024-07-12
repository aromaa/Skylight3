using System.Collections.Frozen;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Public;
using Skylight.Server.Game.Navigator.Nodes;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorManager
{
	private sealed class Cache
	{
		internal FrozenDictionary<string, IRoomLayout> Layouts { get; }
		internal FrozenDictionary<int, INavigatorNode> Nodes { get; }

		private Cache(Dictionary<string, IRoomLayout> layouts, Dictionary<int, INavigatorNode> flatCats)
		{
			this.Layouts = layouts.ToFrozenDictionary();
			this.Nodes = flatCats.ToFrozenDictionary();
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<string, RoomLayoutEntity> layouts;
			private readonly Dictionary<int, PublicRoomEntity> publicRooms;
			private readonly Dictionary<int, NavigatorNodeEntity> rootNodes;

			internal Builder()
			{
				this.layouts = [];
				this.publicRooms = [];
				this.rootNodes = [];
			}

			internal void AddLayout(RoomLayoutEntity layout)
			{
				this.layouts.Add(layout.Id, layout);
			}

			internal void AddPublicRoom(PublicRoomEntity publicRoom)
			{
				this.publicRooms.Add(publicRoom.Id, publicRoom);
			}

			internal void AddFlatCat(NavigatorNodeEntity node)
			{
				if (node.ParentId is null)
				{
					this.rootNodes.Add(node.Id, node);
				}
			}

			internal Cache ToImmutable()
			{
				Dictionary<string, IRoomLayout> layouts = [];
				foreach (RoomLayoutEntity entity in this.layouts.Values)
				{
					RoomLayout layout = new(entity.Id, entity.HeightMap, entity.DoorX, entity.DoorY, entity.DoorDirection);

					layouts.Add(layout.Id, layout);
				}

				Dictionary<int, INavigatorNode> nodes = [];
				foreach (NavigatorNodeEntity entity in this.rootNodes.Values)
				{
					AddNode(entity);
				}

				return new Cache(layouts, nodes);

				NavigatorNode AddNode(NavigatorNodeEntity entity, INavigatorNode? parent = null)
				{
					NavigatorNode node;
					if (entity is NavigatorCategoryNodeEntity category)
					{
						node = new NavigatorCategoryNode((INavigatorCategoryNode?)parent, category.Id, category.Caption);
					}
					else if (entity is NavigatorPublicRoomNodeEntity publicRoomNode)
					{
						PublicRoomEntity publicRoom = this.publicRooms[publicRoomNode.RoomId];

						node = new NavigatorPublicRoomNode(parent, publicRoomNode.Id, publicRoomNode.Caption, publicRoom.Name, publicRoomNode.RoomId, publicRoomNode.WorldId, [.. publicRoom.Casts]);
					}
					else
					{
						throw new NotSupportedException();
					}

					List<INavigatorNode> children = [];
					foreach (NavigatorNodeEntity childEntity in entity.Children!)
					{
						children.Add(AddNode(childEntity, node));
					}

					if (node is NavigatorCategoryNode categoryNode)
					{
						categoryNode.Children = [.. children];
					}

					nodes.Add(node.Id, node);

					return node;
				}
			}
		}
	}
}
