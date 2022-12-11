using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Inventory.Items.Wall.Factory;

internal sealed class StickyNoteInventoryItemFactory : FurnitureInventoryItemFactory<IStickyNoteFurniture, IStickyNoteInventoryItem, int>
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public StickyNoteInventoryItemFactory(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	public override IStickyNoteInventoryItem Create(int itemId, IUserInfo owner, IStickyNoteFurniture furniture, int extraData)
	{
		return new StickyNoteInventoryItem(this.dbContextFactory, itemId, owner, furniture, extraData);
	}

	public override IStickyNoteInventoryItem Create(int itemId, IUserInfo owner, IStickyNoteFurniture furniture, JsonDocument? extraData)
	{
		return new StickyNoteInventoryItem(this.dbContextFactory, itemId, owner, furniture, extraData?.RootElement.GetInt32() ?? 1);
	}
}
