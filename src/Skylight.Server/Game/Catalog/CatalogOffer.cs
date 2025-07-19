using System.Collections.Frozen;
using System.Collections.Immutable;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogOffer : ICatalogOffer
{
	public int Id { get; }
	internal int OrderNum { get; }

	public string Name { get; }

	public IPermissionSubject? PermissionRequirement { get; }

	public FrozenDictionary<ICurrency, int> Cost { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	internal CatalogOffer(int id, int orderNum, string name, IPermissionSubject? permissionSubject, FrozenDictionary<ICurrency, int> cost, TimeSpan rentTime, bool hasOffer, ImmutableArray<ICatalogProduct> products)
	{
		this.Id = id;
		this.OrderNum = orderNum;

		this.Name = name;

		this.PermissionRequirement = permissionSubject;

		this.Cost = cost;

		this.RentTime = rentTime;

		this.HasOffer = hasOffer;

		this.Products = products;
	}

	public bool CanPurchase(IUser user) => this.PermissionRequirement is not { } permissionRequirement || user.PermissionSubject.IsChildOf(permissionRequirement.Reference);

	public ValueTask PurchaseAsync(ICatalogTransactionContext context, CancellationToken cancellationToken)
	{
		foreach ((ICurrency currency, int amount) in this.Cost)
		{
			context.Constraints.DeductCurrency(currency, amount);
		}

		context.AddCommand(async (context, _, cancellationToken) =>
			await this.ClaimAsync(context, cancellationToken).ConfigureAwait(false));

		return ValueTask.CompletedTask;
	}

	public async ValueTask ClaimAsync(ICatalogTransactionContext context, CancellationToken cancellationToken)
	{
		foreach (ICatalogProduct product in this.Products)
		{
			ValueTask task = product.ClaimAsync(context, cancellationToken);
			if (task.IsCompletedSuccessfully)
			{
				continue;
			}

			await task.ConfigureAwait(false);
		}
	}

	public bool CanEffort(IPurse purse)
	{
		foreach ((ICurrency currency, int amount) in this.Cost)
		{
			if (purse.GetBalance(currency) < amount)
			{
				return false;
			}
		}

		return true;
	}
}
