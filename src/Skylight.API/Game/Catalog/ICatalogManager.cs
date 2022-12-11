namespace Skylight.API.Game.Catalog;

public interface ICatalogManager : ICatalogSnapshot
{
	public ICatalogSnapshot Current { get; }

	public Task LoadAsync(CancellationToken cancellationToken = default);
}
