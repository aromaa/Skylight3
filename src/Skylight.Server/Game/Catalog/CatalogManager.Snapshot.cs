using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Catalog;

internal partial class CatalogManager
{
	private sealed class Snapshot : ICatalogSnapshot
	{
		private readonly ICatalogTransactionFactory catalogTransactionFactory;
		private readonly Cache cache;

		internal Snapshot(ICatalogTransactionFactory catalogTransactionFactory, Cache cache)
		{
			this.catalogTransactionFactory = catalogTransactionFactory;
			this.cache = cache;
		}

		public ImmutableArray<ICatalogPage> RootPages => this.cache.RootPages;

		public bool TryGetPage(int pageId, [NotNullWhen(true)] out ICatalogPage? page) => this.cache.Pages.TryGetValue(pageId, out page);
		public bool TryGetOffer(int offerId, [NotNullWhen(true)] out ICatalogOffer? offer) => this.cache.Offers.TryGetValue(offerId, out offer);

		public async Task<ICatalogTransactionResult> PurchaseOfferAsync(IUser user, ICatalogOffer offer, string extraData, int amount, CancellationToken cancellationToken)
		{
			ICatalogTransactionResult result;
			await using (ICatalogTransaction transaction = await this.catalogTransactionFactory.CreateTransactionAsync(this.cache.CurrencyRegistry, this.cache.Furnitures, user, extraData, cancellationToken).ConfigureAwait(false))
			{
				for (int i = 0; i < amount; i++)
				{
					await offer.PurchaseAsync(transaction.Context, cancellationToken).ConfigureAwait(false);
				}

				result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}

			if (result.Result == ICatalogTransactionResult.ResultType.Success)
			{
				user.SendAsync(new PurchaseOKOutgoingPacket(offer.BuildOfferData(this.cache.CurrencyRegistry)));
			}

			foreach ((ICurrency currency, int balance) in result.BalanceUpdate)
			{
				user.Purse.SetBalance(currency, balance);
			}

			user.Inventory.AddUnseenItems(result.CreatedItems);

			return result;
		}
	}
}
