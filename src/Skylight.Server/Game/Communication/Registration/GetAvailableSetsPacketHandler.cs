using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Registration;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Registration;

namespace Skylight.Server.Game.Communication.Registration;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetAvailableSetsPacketHandler<T> : ClientPacketHandler<T>
	where T : IGetAvailableSetsIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		client.SendAsync(new AvailableSetsOutgoingPacket([100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 176, 177, 178, 180, 185, 190, 195, 200, 205, 206, 207, 210, 215, 220, 225, 230, 235, 240, 245, 250, 255, 260, 265, 266, 267, 270, 275, 280, 281, 285, 290, 295, 300, 305, 500, 505, 510, 515, 520, 525, 530, 535, 540, 545, 550, 555, 565, 570, 575, 580, 585, 590, 595, 596, 600, 605, 610, 615, 620, 625, 626, 627, 630, 635, 640, 645, 650, 655, 660, 665, 667, 669, 670, 675, 680, 685, 690, 695, 696, 700, 705, 710, 715, 720, 725, 730, 735, 740]));
	}
}
