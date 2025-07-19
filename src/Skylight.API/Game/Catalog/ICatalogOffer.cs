using System.Collections.Frozen;
using System.Collections.Immutable;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalogOffer
{
	public int Id { get; }

	public string Name { get; }

	public IPermissionSubject? PermissionRequirement { get; }

	public FrozenDictionary<ICurrency, int> Cost { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	public bool CanEffort(IPurse purse);

	public bool CanPurchase(IUser user);

	public ValueTask PurchaseAsync(ICatalogTransactionContext context, CancellationToken cancellationToken = default);
	public ValueTask ClaimAsync(ICatalogTransactionContext context, CancellationToken cancellationToken = default);
}
