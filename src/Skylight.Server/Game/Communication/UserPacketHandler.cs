using System.Runtime.CompilerServices;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming;

namespace Skylight.Server.Game.Communication;

internal abstract class UserPacketHandler<T> : ClientPacketHandler<T>
	where T : IGameIncomingPacket
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal sealed override void Handle(IClient client, in T packet)
	{
		if (client.User is { } user)
		{
			this.Handle(user, packet);
		}
	}

	internal abstract void Handle(IUser user, in T packet);
}
