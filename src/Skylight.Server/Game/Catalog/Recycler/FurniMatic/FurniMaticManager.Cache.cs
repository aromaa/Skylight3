using System.Collections.Immutable;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal partial class FurniMaticManager
{
	private sealed class Cache
	{
		internal FurniMaticPrizes Prizes { get; }

		internal IFloorFurniture? GiftFurniture { get; }

		private Cache(FurniMaticPrizes prizes, IFloorFurniture? giftFurniture)
		{
			this.Prizes = prizes;

			this.GiftFurniture = giftFurniture;
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<int, FurniMaticPrizeLevelEntity> prizeLevels;

			internal IFloorFurniture? GiftFurniture { get; set; }

			internal Builder()
			{
				this.prizeLevels = [];
			}

			internal void AddLevel(FurniMaticPrizeLevelEntity prizeLevel)
			{
				this.prizeLevels.Add(prizeLevel.Level, prizeLevel);
			}

			internal Cache ToImmutable(IFurnitureSnapshot furnitures)
			{
				ImmutableArray<IFurniMaticPrizeLevel>.Builder prizeLevels = ImmutableArray.CreateBuilder<IFurniMaticPrizeLevel>(this.prizeLevels.Count);
				foreach (FurniMaticPrizeLevelEntity prizeLevel in this.prizeLevels.Values)
				{
					if (prizeLevel.Prizes is not { Count: > 0 })
					{
						throw new InvalidOperationException($"The level {prizeLevel.Level} has no prizes!");
					}

					ImmutableArray<IFurniMaticPrize>.Builder levelPrizes = ImmutableArray.CreateBuilder<IFurniMaticPrize>(prizeLevel.Prizes.Count);
					foreach (FurniMaticPrizeEntity prize in prizeLevel.Prizes)
					{
						if (prize.Items is not { Count: > 0 })
						{
							throw new InvalidOperationException($"The prize {prize.Id} has no items!");
						}

						ImmutableArray<IFurniture>.Builder prizeItems = ImmutableArray.CreateBuilder<IFurniture>(prize.Items.Count);
						foreach (FurniMaticItemEntity item in prize.Items)
						{
							if (item is FurniMaticFloorItemEntity floorItem)
							{
								if (furnitures.TryGetFloorFurniture(floorItem.FurnitureId, out IFloorFurniture? furniture))
								{
									prizeItems.Add(furniture);
								}
								else
								{
									throw new InvalidOperationException($"The prize item {item.Id} is referring to non-existent floor item {floorItem.FurnitureId}!");
								}
							}
							else if (item is FurniMaticWallItemEntity wallItem)
							{
								if (furnitures.TryGetWallFurniture(wallItem.FurnitureId, out IWallFurniture? furniture))
								{
									prizeItems.Add(furniture);
								}
								else
								{
									throw new InvalidOperationException($"The prize item {item.Id} is referring to non-existent wall item {wallItem.FurnitureId}!");
								}
							}
							else
							{
								throw new InvalidOperationException($"The prize item {item.Id} is missing a floor or wall furniture!");
							}
						}

						levelPrizes.Add(new FurniMaticPrize(prize.Id, prize.Name, prizeItems.MoveToImmutable()));
					}

					prizeLevels.Add(new FurniMaticPrizeLevel(prizeLevel.Level, prizeLevel.Odds, levelPrizes.MoveToImmutable()));
				}

				prizeLevels.Sort((x, y) => x.Level.CompareTo(y.Level));

				return new Cache(new FurniMaticPrizes(prizeLevels.MoveToImmutable()), this.GiftFurniture);
			}
		}
	}
}
