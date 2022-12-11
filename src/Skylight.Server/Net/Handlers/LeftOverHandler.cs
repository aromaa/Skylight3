using Net.Sockets.Pipeline.Handler;
using Net.Sockets.Pipeline.Handler.Incoming;

namespace Skylight.Server.Net.Handlers;

internal sealed class LeftOverHandler : IIncomingObjectHandler
{
	public void Handle<T>(IPipelineHandlerContext context, ref T packet)
	{
		Console.WriteLine($"Unhandled packet: {typeof(T)}");
	}
}
