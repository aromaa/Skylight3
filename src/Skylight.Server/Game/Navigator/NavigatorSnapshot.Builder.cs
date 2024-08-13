using System.Collections.Frozen;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Public;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Navigator.Nodes;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorSnapshot
{
	internal static Builder CreateBuilder() => new();

	internal sealed class Builder : Builder<NavigatorSnapshot>
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

		private Cache BuildCache()
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

			return new Cache(layouts.ToFrozenDictionary(), nodes.ToFrozenDictionary());

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

		private Holders BuildHolders(in Cache cache, VersionedLoadableServiceBase? instance = null, NavigatorSnapshot? current = null)
		{
			Dictionary<int, IServiceValue<INavigatorNode>> nodeHolders = [];
			foreach ((int id, INavigatorNode node) in cache.Nodes)
			{
				if (instance is not null)
				{
					IServiceValue<INavigatorNode> holder = node switch
					{
						INavigatorCategoryNode category => ResolveNode(category, instance, current),
						INavigatorPublicRoomNode publicRoom => ResolveNode(publicRoom, instance, current),
						INavigatorPrivateRoomNode privateRoom => ResolveNode(privateRoom, instance, current),

						_ => throw new NotSupportedException()
					};

					nodeHolders.Add(id, holder);
				}
			}

			return new Holders(nodeHolders.ToFrozenDictionary());

			static IServiceValue<INavigatorNode> ResolveNode<T>(T node, VersionedLoadableServiceBase instance, NavigatorSnapshot? current = null)
				where T : class, INavigatorNode
			{
				if (current is not null && current.TryGetNode(node.Id, out IServiceValue<INavigatorNode>? holder) && holder is ServiceValue<T> holderOfT)
				{
					holderOfT.StartTransaction(instance, current.Version, node);
				}
				else
				{
					holder = new ServiceValue<T>(node);
				}

				return holder;
			}
		}

		internal override NavigatorSnapshot Build()
		{
			Cache cache = this.BuildCache();

			return new NavigatorSnapshot(cache, this.BuildHolders(cache));
		}

		internal override Transaction<NavigatorSnapshot> BuildAndStartTransaction(VersionedLoadableServiceBase instance, NavigatorSnapshot current)
		{
			Cache cache = this.BuildCache();

			return new Transaction(new NavigatorSnapshot(cache, this.BuildHolders(cache, instance, current)));
		}

		private sealed class Transaction(NavigatorSnapshot current) : Transaction<NavigatorSnapshot>(current)
		{
			internal override NavigatorSnapshot Commit(int version)
			{
				this.Current.Version = version;

				return this.Current;
			}

			public override void Dispose()
			{
				foreach (ServiceValue holder in this.Current.holders.Nodes.Values)
				{
					holder.Commit();
				}
			}
		}
	}
}
