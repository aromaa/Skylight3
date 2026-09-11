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
		string figureData = Encoding.ASCII.GetString(packet.Figure);

		user.Client.ScheduleTask(async _ =>
		{
			IFigureConfigurationSnapshot figureValidator = await this.figureConfigurationManager.GetAsync().ConfigureAwait(false);

			figureValidator.TryGetFigureValidator("user", sex, out IFigureValidator? validator);

			IFigureDataContainer figure = figureValidator.Parse(figureData, new FigureValidationOptions(validator, user.PermissionSubject));

			user.Info.Avatar = new FigureAvatar(sex, figure);
		});
	}
}
