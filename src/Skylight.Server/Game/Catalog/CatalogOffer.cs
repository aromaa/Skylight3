using System.Collections.Immutable;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogOffer : ICatalogOffer
{
	public int Id { get; }
	internal int OrderNum { get; }

	public string Name { get; }

	public IPermissionSubject? PermissionRequirement { get; }

	public int CostCredits { get; }
	public int CostActivityPoints { get; }
	public int ActivityPointsType { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	internal CatalogOffer(int id, int orderNum, string name, IPermissionSubject? permissionSubject, int costCredits, int costActivityPoints, int activityPointsType, TimeSpan rentTime, bool hasOffer, ImmutableArray<ICatalogProduct> products)
	{
		this.Id = id;
		this.OrderNum = orderNum;

		this.Name = name;

		this.PermissionRequirement = permissionSubject;

		this.CostCredits = costCredits;
		this.CostActivityPoints = costActivityPoints;

		this.ActivityPointsType = activityPointsType;

		this.RentTime = rentTime;

		this.HasOffer = hasOffer;

		this.Products = products;
	}

	public bool CanPurchase(IUser user) => this.PermissionRequirement is not { } permissionRequirement || user.PermissionSubject.IsChildOf(permissionRequirement.Reference);

	public async ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken)
	{
		foreach (ICatalogProduct product in this.Products)
		{
			ValueTask task = product.PurchaseAsync(transaction, cancellationToken);

			if (task.IsCompletedSuccessfully)
			{
				continue;
			}

			await task.ConfigureAwait(false);
		}
	}
}
