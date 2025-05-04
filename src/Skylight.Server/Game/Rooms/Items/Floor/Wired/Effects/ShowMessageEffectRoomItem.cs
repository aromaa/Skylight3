using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.Room.Chat;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

internal sealed class ShowMessageEffectRoomItem(IPrivateRoom room, int id, IUserInfo owner, IShowMessageEffectFurniture furniture, Point3D position, int direction, IWiredEffectInteractionHandler interactionHandler,
	string message, int effectDelay)
	: WiredEffectRoomItem<IShowMessageEffectFurniture>(room, id, owner, furniture, position, direction, effectDelay), IShowMessageEffectRoomItem
{
	private readonly IWiredEffectInteractionHandler interactionHandler = interactionHandler;

	public new IShowMessageEffectFurniture Furniture => this.furniture;

	public string Message { get; set; } = message;

	public override void OnPlace()
	{
		this.interactionHandler.OnPlace(this);
	}

	public override void OnMove(Point3D position, int direction)
	{
		this.interactionHandler.OnMove(this, position);

		base.OnMove(position, direction);
	}

	public override void OnRemove()
	{
		this.interactionHandler.OnRemove(this);
	}

	public override void Open(IUserRoomUnit unit)
	{
		unit.User.SendAsync(new WiredFurniActionOutgoingPacket(this.Id, this.Furniture.Id, ActionType.ShowMessage, 0, [], 0, [], this.Message));
	}

	public override void Trigger(IUserRoomUnit? cause = null)
	{
		cause?.User.SendAsync(new WhisperOutgoingPacket(cause.Id, cause.User.Profile.Username, this.Message, 0, 34, -1, Array.Empty<(string, string, bool)>()));
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			this.Message,
			this.EffectDelay
		});
	}
}
