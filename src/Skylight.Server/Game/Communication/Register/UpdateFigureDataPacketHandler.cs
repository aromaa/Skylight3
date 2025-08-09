using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Register;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Register;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class UpdateFigureDataPacketHandler<T>(IFigureConfigurationManager figureConfigurationManager) : UserPacketHandler<T>
	where T : IUpdateFigureDataIncomingPacket
{
	private readonly IFigureConfigurationManager figureConfigurationManager = figureConfigurationManager;

	internal override void Handle(IUser user, in T packet)
	{
		FigureSex sex = Encoding.ASCII.GetString(packet.Gender) == "M" ? FigureSex.Male : FigureSex.Female;

		this.figureConfigurationManager.TryGetFigureValidator("user", sex, out IFigureValidator? validator);

		IFigureDataContainer figure = this.figureConfigurationManager.Parse(packet.Figure, new FigureValidationOptions(validator, user.PermissionSubject));

		user.Info.Avatar = new FigureAvatar(sex, figure);
	}
}
