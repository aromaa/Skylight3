using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class GetCatalogIndexPacketHandler<T>(ICatalogManager catalogManager) : UserPacketHandler<T>
	where T : IGetCatalogIndexIncomingPacket
{
	private readonly ICatalogManager catalogManager = catalogManager;

	internal override void Handle(IUser user, in T packet)
	{
		//TODO: Caching
		static List<CatalogNodeData> BuildChildren(IEnumerable<ICatalogPage> pages)
		{
			List<CatalogNodeData> nodes = [];

			foreach (ICatalogPage page in pages)
			{
				nodes.Add(new CatalogNodeData
				{
					Id = !page.Enabled
						? -1 //Modern client avoids opening the page for negative ids but that's not true for old client, however it doesn't break them.
						: page.Id,
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

		string catalogType = Encoding.ASCII.GetString(packet.CatalogType);

		user.Client.ScheduleTask(async client =>
		{
			ICatalogSnapshot catalog = await this.catalogManager.GetAsync().ConfigureAwait(false);

			client.SendAsync(new CatalogIndexOutgoingPacket(catalogType, new CatalogNodeData
			{
				Id = -1,
				Visible = true,
				Icon = 0,
				Color = 0,
				Name = "root",
				Localization = "Root",
				OfferIds = Array.Empty<int>(),
				Children = BuildChildren(catalog.RootPages)
			}, false));
		});
	}
}
