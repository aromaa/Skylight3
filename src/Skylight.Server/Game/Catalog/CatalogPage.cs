using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Catalog;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogPage : ICatalogPage
{
	public int Id { get; }

	public string Name { get; }
	public string Localization { get; }

	internal int OrderNum { get; }

	public bool Enabled { get; }
	public bool Visible { get; }

	internal int MinRank { get; }
	internal int ClubRank { get; }

	public int IconColor { get; }
	public int IconImage { get; }

	public string Layout { get; }

	public ImmutableArray<string> Texts { get; }
	public ImmutableArray<string> Images { get; }

	public bool AcceptSeasonCurrencyAsCredits { get; }

	private readonly Dictionary<int, ICatalogOffer> offers;
	private readonly OrderedDictionary<int, ICatalogPage> children;

	internal CatalogPage(int id, string name, string localization, int orderNum, bool enabled, bool visible, int minRank, int clubRank, int iconColor, int iconImage, string layout, ImmutableArray<string> texts, ImmutableArray<string> images, bool acceptSeasonCurrencyAsCredits, Dictionary<int, ICatalogOffer> offers, OrderedDictionary<int, ICatalogPage> children)
	{
		this.Id = id;

		this.Name = name;
		this.Localization = localization;

		this.OrderNum = orderNum;

		this.Enabled = enabled;
		this.Visible = visible;

		this.MinRank = minRank;
		this.ClubRank = clubRank;

		this.IconColor = iconColor;
		this.IconImage = iconImage;

		this.Layout = layout;

		this.Texts = texts;
		this.Images = images;

		this.AcceptSeasonCurrencyAsCredits = acceptSeasonCurrencyAsCredits;

		this.offers = offers;
		this.children = children;
	}

	public IEnumerable<ICatalogOffer> Offers => this.offers.Values;
	public IEnumerable<ICatalogPage> Children => this.children.Values;

	public bool TryGetOffer(int offerId, [NotNullWhen(true)] out ICatalogOffer? offer) => this.offers.TryGetValue(offerId, out offer);
}
