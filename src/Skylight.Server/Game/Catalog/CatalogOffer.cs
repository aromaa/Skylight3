using System.Collections.Immutable;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;

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
	private readonly ICurrency priceCurrency;

	internal CatalogOffer(int id, string name, int orderNum, int clubRank, int costCredits, int costActivityPoints, int activityPointsType, TimeSpan rentTime, bool hasOffer, ImmutableArray<ICatalogProduct> products, ICurrency priceCurrency)
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

		this.priceCurrency = priceCurrency;
	}

	private bool CanPurchase(ICatalogTransaction tx) =>
		tx.GetCurrencyBalance(this.priceCurrency) >= this.CostCredits;

	public async ValueTask PurchaseAsync(
		ICatalogTransaction transaction,
		CancellationToken cancellationToken)
	{
		if (this.CostCredits < 0)
		{
			throw new InvalidOperationException("Offer cost must be non-negative.");
		}

		if (!this.CanPurchase(transaction))
		{
			throw new InvalidOperationException("Insufficient credits for this purchase.");
		}

		if (this.CostCredits > 0)
		{
			transaction.DeductCurrency(this.priceCurrency, this.CostCredits);
		}

		IEnumerable<Task> purchaseTasks = this.Products
			.Select(p => p.PurchaseAsync(transaction, cancellationToken).AsTask());

		await Task.WhenAll(purchaseTasks).ConfigureAwait(false);
	}
}
