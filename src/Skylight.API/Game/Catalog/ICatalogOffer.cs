using System.Collections.Immutable;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalogOffer
{
	public int Id { get; }

	public string Name { get; }

	public IPermissionSubject? PermissionRequirement { get; }

	public int CostCredits { get; }
	public int CostActivityPoints { get; }
	public int ActivityPointsType { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	public bool CanPurchase(IUser user);

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default);
}
