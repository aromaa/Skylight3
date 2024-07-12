namespace Skylight.API.Game.Navigator.Nodes;

public interface INavigatorNode
{
	public INavigatorNode? Parent { get; }

	public int Id { get; }

	public string Caption { get; }
}
