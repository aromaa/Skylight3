using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetCatalogIndexPacketHandler<T> : UserPacketHandler<T>
	where T : IGetCatalogIndexIncomingPacket
{
	private readonly ICatalogManager catalogManager;

	public GetCatalogIndexPacketHandler(ICatalogManager catalogManager)
	{
		this.catalogManager = catalogManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		//TODO: Caching
		static List<CatalogNodeData> BuildChildren(IEnumerable<ICatalogPage> pages)
		{
			List<CatalogNodeData> nodes = new();

			foreach (ICatalogPage page in pages)
			{
				nodes.Add(new CatalogNodeData
				{
					Id = page.Id,
					Name = page.Name,
					Localization = page.Localization,
					Visible = page.Visible,
					Color = page.IconColor,
					Icon = page.IconImage,
					OfferIds = page.Offers.Select(o => o.Id).ToList(),
					Children = BuildChildren(page.Children)
				});
			}

			return nodes;
		}

		user.SendAsync(new CatalogIndexOutgoingPacket(Encoding.UTF8.GetString(packet.CatalogType), new CatalogNodeData
		{
			Id = 0,
			Visible = true,
			Icon = 0,
			Color = 0,
			Name = "root",
			Localization = "Root",
			OfferIds = Array.Empty<int>(),
			Children = BuildChildren(this.catalogManager.RootPages)
		}, false));
	}
}
