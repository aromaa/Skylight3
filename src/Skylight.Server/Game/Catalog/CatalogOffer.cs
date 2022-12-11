using System.Collections.Immutable;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogOffer : ICatalogOffer
{
	public int Id { get; }

	public string Name { get; }

	internal int OrderNum { get; }

	public int ClubRank { get; }

	public int CostCredits { get; }
	public int CostActivityPoints { get; }
	public int ActivityPointsType { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	internal CatalogOffer(int id, string name, int orderNum, int clubRank, int costCredits, int costActivityPoints, int activityPointsType, TimeSpan rentTime, bool hasOffer, ImmutableArray<ICatalogProduct> products)
	{
		this.Id = id;

		this.Name = name;

		this.OrderNum = orderNum;

		this.ClubRank = clubRank;

		this.CostCredits = costCredits;
		this.CostActivityPoints = costActivityPoints;

		this.ActivityPointsType = activityPointsType;

		this.RentTime = rentTime;

		this.HasOffer = hasOffer;

		this.Products = products;
	}

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
