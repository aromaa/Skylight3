using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Catalog;

internal partial class CatalogManager
{
	public ImmutableArray<ICatalogPage> RootPages => this.Current.RootPages;

	public bool TryGetPage(int pageId, [NotNullWhen(true)] out ICatalogPage? page) => this.Current.TryGetPage(pageId, out page);

	public Task PurchaseOfferAsync(IUser user, ICatalogOffer offer, string extraData, int amount, CancellationToken cancellationToken) => this.Current.PurchaseOfferAsync(user, offer, extraData, amount, cancellationToken);

	private sealed class Snapshot : ICatalogSnapshot
	{
		private readonly CatalogManager catalogManager;
		private readonly Cache cache;

		internal Snapshot(CatalogManager catalogManager, Cache cache)
		{
			this.catalogManager = catalogManager;
			this.cache = cache;
		}

		public ImmutableArray<ICatalogPage> RootPages => this.cache.RootPages;

		public bool TryGetPage(int pageId, [NotNullWhen(true)] out ICatalogPage? page) => this.cache.Pages.TryGetValue(pageId, out page);

		public async Task PurchaseOfferAsync(IUser user, ICatalogOffer offer, string extraData, int amount, CancellationToken cancellationToken)
		{
			await using ICatalogTransaction transaction = await this.catalogManager.catalogTransactionFactory.CreateTransactionAsync(this.cache.Furnitures, user, extraData, cancellationToken).ConfigureAwait(false);

			for (int i = 0; i < amount; i++)
			{
				await offer.PurchaseAsync(transaction, cancellationToken).ConfigureAwait(false);
			}

			await transaction.CompleteAsync(cancellationToken).ConfigureAwait(false);

			user.SendAsync(new PurchaseOKOutgoingPacket(offer.BuildOfferData()));
		}
	}
}
