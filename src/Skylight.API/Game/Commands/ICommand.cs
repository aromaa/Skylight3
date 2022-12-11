namespace Skylight.API.Game.Commands;

public interface ICommand
{
	//TODO: Span<Span<char>> arguments
	public void Execute(ICommandExecutor executor, string[] arguments);
}
