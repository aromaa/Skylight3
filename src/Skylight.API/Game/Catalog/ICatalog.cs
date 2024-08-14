using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalog
{
	public ImmutableArray<ICatalogPage> RootPages { get; }

	public bool TryGetPage(int pageId, [NotNullWhen(true)] out ICatalogPage? page);
	public bool TryGetOffer(int offerId, [NotNullWhen(true)] out ICatalogOffer? offer);

	public Task PurchaseOfferAsync(IUser user, ICatalogOffer offer, string extraData, int amount, CancellationToken cancellationToken = default);
}
