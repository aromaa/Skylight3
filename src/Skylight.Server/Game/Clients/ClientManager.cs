using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Net.Collections;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net;
using Skylight.Server.Net.Listener.Connection;

namespace Skylight.Server.Game.Clients;

internal sealed class ClientManager : IClientManager
{
	private readonly IUserAuthentication userAuthentication;

	private readonly CriticalSocketCollection<IClient> clients;

	private readonly ConcurrentDictionary<int, UserHolder> users;

	public ClientManager(IUserAuthentication userAuthentication, IOptions<NetworkSettings> networkOptions)
	{
		this.userAuthentication = userAuthentication;

		this.users = [];

		this.clients = new CriticalSocketCollection<IClient>(removeEvent: (ISocket _, ref IClient client) =>
		{
			client.Disconnect();

			if (client.User is { } user)
			{
				this.users.TryRemove(KeyValuePair.Create(user.Id, new UserHolder(user)));
			}
		});

		NetworkSettings settings = networkOptions.Value;
		if (settings.TimeoutInSeconds > 0)
		{
			_ = this.TimeoutTask(settings);
		}
	}

	public bool TryAccept(IClient client)
	{
		return this.clients.TryAdd(client.Socket, client);
	}

	public async Task<bool> LoginAsync(IClient client, int userId)
	{
		UserHolder holder = this.users.AddOrUpdate(userId, new UserHolder(), static (_, oldValue) => oldValue.CreateNewAndChain());

		await holder.PrepareLoginAsync().ConfigureAwait(false);

		IUser? user = await this.userAuthentication.LoginAsync(client, userId).ConfigureAwait(false);
		if (user is null || client.Socket.Closed)
		{
			this.users.TryRemove(KeyValuePair.Create(userId, holder));

			return false;
		}

		if (!this.users.TryUpdate(userId, new UserHolder(user), holder))
		{
			return false;
		}

		client.Authenticate(user);

		if (client.Socket.Closed)
		{
			this.users.TryRemove(KeyValuePair.Create(userId, new UserHolder(user)));

			return false;
		}

		return true;
	}

	private async Task TimeoutTask(NetworkSettings networkSettings) //TODO: Move this to the network library?
	{
		long timeoutMilliseconds = (long)TimeSpan.FromSeconds(networkSettings.TimeoutInSeconds).TotalMilliseconds;

		PingOutgoingPacket packet = new();

		PeriodicTimer timer = new(TimeSpan.FromSeconds(networkSettings.TimeoutInSeconds / 2.0));
		while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
		{
			long now = Environment.TickCount64;

			foreach (ISocket socket in this.clients.Values)
			{
				//TODO: Remove
				if (!socket.Metadata.TryGetValue(NetworkConnectionHandler.GameClientMetadataKey, out Client? client))
				{
					continue;
				}

				if (now - client.LastPongReceived <= timeoutMilliseconds)
				{
					client.SendAsync(packet);
				}
				else
				{
					socket.Disconnect("Timeout");
				}
			}
		}
	}

	private readonly struct UserHolder : IEquatable<UserHolder>
	{
		private readonly object value;

		public UserHolder()
		{
			this.value = new UserLoading();
		}

		internal UserHolder(IUser user)
		{
			this.value = user;
		}

		private UserHolder(UserLoading loader)
		{
			this.value = loader;
		}

		internal IUser? User
		{
			get
			{
				//Structs can tear
				object value = this.value;

				return value.GetType() == typeof(UserLoading)
					? Unsafe.As<UserLoading>(value).OldUser
					: Unsafe.As<IUser>(value);
			}
		}

		internal UserHolder CreateNewAndChain() => new(new UserLoading(this.User));

		internal Task PrepareLoginAsync()
		{
			if (this.User?.Client.Socket is not { } socket || socket.Closed)
			{
				return Task.CompletedTask;
			}

			TaskCompletionSource taskCompletionSource = new();

			socket.Disconnect("Logged in from another place");
			socket.OnDisconnected += _ => taskCompletionSource.SetResult();

			return taskCompletionSource.Task;
		}

		public bool Equals(UserHolder other) => this.value == other.value;

		public override bool Equals(object? obj) => obj is UserHolder other && this.Equals(other);
		public override int GetHashCode() => this.value.GetHashCode();

		private sealed class UserLoading(IUser? oldUser = null)
		{
			internal IUser? OldUser => oldUser;
		}
	}
}
