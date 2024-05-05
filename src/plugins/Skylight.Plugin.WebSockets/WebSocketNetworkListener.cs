using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using Net.Buffers;
using Net.Metadata;
using Net.Sockets;
using Net.Sockets.Pipeline;
using Skylight.API.Net.Connection;
using Skylight.API.Net.Listener;

namespace Skylight.Plugin.WebSockets;

internal sealed class WebSocketNetworkListener(INetworkConnectionHandler connectionHandler, Uri endPoint) : INetworkListener
{
	private readonly INetworkConnectionHandler connectionHandler = connectionHandler;

	private readonly Uri endPoint = endPoint;

	private NetworkListenerConfiguration? configuration;

	public void Start(NetworkListenerConfiguration configuration)
	{
		this.configuration = configuration;

		UriBuilder uriBuilder = new(this.endPoint)
		{
			Scheme = Uri.UriSchemeHttp
		};

		HttpListener listener = new();
		listener.Prefixes.Add(uriBuilder.ToString());
		listener.Start();

		_ = this.AcceptLoopAsync(listener);
	}

	private async Task AcceptLoopAsync(HttpListener listener)
	{
		while (true)
		{
			try
			{
				HttpListenerContext listenerContext = await listener.GetContextAsync().ConfigureAwait(false);
				if (listenerContext.Request.IsWebSocketRequest)
				{
					_ = this.ConnectionLoopAsync(listenerContext);
				}
				else
				{
					listenerContext.Response.StatusCode = 400;
					listenerContext.Response.Close();
				}
			}
			catch
			{
				//Log
			}
		}
	}

	private async Task ConnectionLoopAsync(HttpListenerContext listenerContext)
	{
		try
		{
			WebSocketContext webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null).ConfigureAwait(false);

			using WebSocket webSocket = webSocketContext.WebSocket;
			using WebSocketWrapper socket = new(webSocket, listenerContext.Request.LocalEndPoint, listenerContext.Request.RemoteEndPoint);

			this.connectionHandler.Accept(socket, this.configuration!.Revision!, this.configuration.CryptoPrime, this.configuration.CryptoGenerator, this.configuration.CryptoKey, this.configuration.CryptoPremix);

			byte[] buffer = new byte[1024 * 6];

			while (webSocket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, default).ConfigureAwait(false);
				if (result.MessageType == WebSocketMessageType.Close)
				{
					socket.Disconnect();
					break;
				}
				else if (result.MessageType == WebSocketMessageType.Text)
				{
					socket.Disconnect();
					break;
				}

				Read(socket, buffer, result.Count);

				static void Read(WebSocketWrapper socket, byte[] buffer, int amount)
				{
					PacketReader reader = new(new ReadOnlySequence<byte>(buffer, 0, amount));

					long consumed = reader.Consumed;

					while (true)
					{
						socket.Pipeline.Read(ref reader);

						if (reader.End || consumed == reader.Consumed)
						{
							break;
						}

						consumed = reader.Consumed;
					}
				}
			}
		}
		catch
		{
			listenerContext.Response.StatusCode = 500;
			listenerContext.Response.Close();
		}
	}

	public void Stop()
	{
	}

	public void Dispose() => this.DisposeAsync().AsTask().GetAwaiter().GetResult();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	private sealed class WebSocketWrapper : ISocket
	{
		private readonly WebSocket webSocket;

		public SocketId Id { get; }

		public SocketPipeline Pipeline { get; }
		public MetadataMap Metadata { get; }

		public EndPoint? LocalEndPoint { get; }
		public EndPoint? RemoteEndPoint { get; }

		private SocketEvent<ISocket>? disconnectedEvent;

		internal WebSocketWrapper(WebSocket webSocket, EndPoint localEndPoint, EndPoint remoteEndPoint)
		{
			this.webSocket = webSocket;

			this.Id = SocketId.GenerateNew();

			this.Pipeline = new SocketPipeline(this);
			this.Metadata = new MetadataMap();

			this.LocalEndPoint = localEndPoint;
			this.RemoteEndPoint = remoteEndPoint;
		}

		public event SocketEvent<ISocket>? OnConnected
		{
			add => throw new NotImplementedException();
			remove => throw new NotImplementedException();
		}

		public event SocketEvent<ISocket>? OnDisconnected
		{
			add => this.disconnectedEvent += value;
			remove => this.disconnectedEvent -= value;
		}

		public ValueTask SendAsync<TPacket>(in TPacket data)
		{
			ArrayBufferWriter<byte> buffer = new();

			PacketWriter packetWriter = new(buffer);
			this.Pipeline.Write(ref packetWriter, data);
			packetWriter.Dispose(flushWriter: false);

			return this.SendBytesAsync(buffer.WrittenMemory);
		}

		public bool Closed => this.webSocket.CloseStatus is not null;

		public ValueTask SendBytesAsync(ReadOnlyMemory<byte> data) => this.webSocket.SendAsync(data, WebSocketMessageType.Binary, true, default);

		public void Disconnect(Exception exception) => this.webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Fault", default);
		public void Disconnect(string? reason = default) => this.webSocket.CloseAsync(WebSocketCloseStatus.Empty, reason, default);

		public void Dispose()
		{
			this.disconnectedEvent?.Invoke(this);
		}
	}
}
