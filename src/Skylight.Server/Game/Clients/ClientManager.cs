using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Net.Collections;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;

namespace Skylight.Server.Game.Clients;

internal sealed class ClientManager : IClientManager
{
	private readonly IUserAuthentication userAuthentication;

	private readonly CriticalSocketCollection<IUser> clients;

	private readonly ConcurrentDictionary<int, UserHolder> users;

	public ClientManager(IUserAuthentication userAuthentication)
	{
		this.userAuthentication = userAuthentication;

		this.users = [];

		this.clients = new CriticalSocketCollection<IUser>(removeEvent: (ISocket _, ref IUser user) =>
		{
			user.Client.Disconnect();

			this.users.TryRemove(KeyValuePair.Create(user.Profile.Id, new UserHolder(user)));
		});
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

		if (!this.users.TryUpdate(userId, new UserHolder(user), holder) || !this.clients.TryAdd(client.Socket, user))
		{
			return false;
		}

		client.Authenticate(user);

		return true;
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

		internal IUser? User => this.value.GetType() == typeof(UserLoading)
			? Unsafe.As<UserLoading>(this.value).OldUser
			: Unsafe.As<IUser>(this.value);

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
