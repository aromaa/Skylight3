﻿using System.Diagnostics.CodeAnalysis;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorManager
{
	public IEnumerable<INavigatorNode> Nodes => this.Current.Nodes;

	public bool TryGetNode<T>(int id, [NotNullWhen(true)] out T? node)
		where T : class, INavigatorNode => this.Current.TryGetNode(id, out node);

	public bool TryGetNode<T>(int nodeId, [NotNullWhen(true)] out IServiceValue<T>? node)
		where T : class, INavigatorNode => this.Current.TryGetNode(nodeId, out node);

	public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.Current.TryGetLayout(id, out layout);
}
