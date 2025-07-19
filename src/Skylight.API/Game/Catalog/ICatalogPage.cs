using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalogPage
{
	public int Id { get; }

	public string Name { get; }
	public string Localization { get; }

	public bool Enabled { get; }
	public bool Visible { get; }

	public int IconColor { get; }
	public int IconImage { get; }

	public string Layout { get; }

	public ImmutableArray<string> Texts { get; }
	public ImmutableArray<string> Images { get; }

	public IEnumerable<ICatalogOffer> Offers { get; }
	public IEnumerable<ICatalogPage> Children { get; }

	public bool CanAccess(IUser user);

	public bool TryGetOffer(int offerId, [NotNullWhen(true)] out ICatalogOffer? offer);
}
