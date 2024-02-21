using Net.Buffers;
using Net.Sockets;
using Net.Sockets.Pipeline.Handler;
using Net.Sockets.Pipeline.Handler.Incoming;

namespace Skylight.Server.Net.Handlers;

internal sealed class FlashSocketPolicyRequestHandler : IncomingBytesHandler
{
	public static readonly FlashSocketPolicyRequestHandler Instance = new();

	private static readonly ReadOnlyMemory<byte> policyFileResponse = "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\"/></cross-domain-policy>\0"u8.ToArray();

	protected override void Decode(IPipelineHandlerContext context, ref PacketReader reader)
	{
		if (reader.SequenceEqual("<policy-file-request/>\0"u8))
		{
			_ = SendSocketPolicy(context.Socket);

			static async Task SendSocketPolicy(ISocket socket)
			{
				await socket.SendBytesAsync(FlashSocketPolicyRequestHandler.policyFileResponse).ConfigureAwait(false);

				socket.Disconnect("Socket policy request");
			}

			return;
		}

		//Only the first data may be the policy request, remove us after that's not the case
		context.Socket.Pipeline.RemoveHandler(this);
		context.Socket.Pipeline.Read(ref reader);
	}
}
