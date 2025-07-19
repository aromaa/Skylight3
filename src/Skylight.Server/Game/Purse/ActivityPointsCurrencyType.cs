using System.Text.Json;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Purse;
using Skylight.Protocol.Packets.Outgoing.Notifications;

namespace Skylight.Server.Game.Purse;

internal sealed class ActivityPointsCurrencyType : ICompoundCurrencyType<IActivityPointsCurrency>
{
	public IActivityPointsCurrency Create(JsonDocument? data = null)
	{
		if (data is null || !data.RootElement.TryGetInt32(out int kind))
		{
			throw new ArgumentException("Data must be a valid JSON document with an integer value", nameof(data));
		}

		return new Impl(this, data, kind);
	}

	private sealed class Impl(ActivityPointsCurrencyType type, JsonDocument data, int kind) : IActivityPointsCurrency, IEquatable<Impl>
	{
		public ICurrencyType Type { get; } = type;
		public JsonDocument Data { get; } = data;

		public int Kind { get; } = kind;

		public void Update(IClient client, int amount) => client.SendAsync(new ActivityPointNotificationOutgoingPacket(this.Kind, amount, 0));

		public bool Equals(Impl? other)
		{
			if (other is null)
			{
				return false;
			}

			if (object.ReferenceEquals(this, other))
			{
				return true;
			}

			return this.Type == other.Type && this.Kind == other.Kind;
		}

		public override bool Equals(object? obj) => obj is Impl other && this.Equals(other);
		public override int GetHashCode() => HashCode.Combine(this.Type, this.Kind);

		public static bool operator ==(Impl? left, Impl? right) => Impl.Equals(left, right);
		public static bool operator !=(Impl? left, Impl? right) => !Impl.Equals(left, right);
	}
}
