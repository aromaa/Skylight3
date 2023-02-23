using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.Server.Collections.Immutable;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal sealed class FurniMaticPrizes : IFurniMaticPrizes
{
	/// <summary>
	/// Gets guaranteed to be sorted by prize level in ascending order.
	/// </summary>
	public ImmutableArray<IFurniMaticPrizeLevel> Levels { get; }

	private readonly ImmutableWeightedTable<IFurniMaticPrize> prizeTable;

	private readonly FrozenDictionary<int, IFurniMaticPrize> prizes;

	internal FurniMaticPrizes(ImmutableArray<IFurniMaticPrizeLevel> levels)
	{
		this.Levels = levels.Sort((x, y) => x.Level.CompareTo(y.Level));

		this.prizeTable = FurniMaticPrizes.CreatePrizeTable(levels);

		this.prizes = levels.SelectMany(x => x.Prizes).ToDictionary(x => x.Id).ToFrozenDictionary(optimizeForReading: true);
	}

	internal IFurniMaticPrize? RollRandomPrice() => this.prizeTable.Next();

	private static ImmutableWeightedTable<IFurniMaticPrize> CreatePrizeTable(ImmutableArray<IFurniMaticPrizeLevel> levels)
	{
		if (levels.Length <= 0)
		{
			return ImmutableWeightedTable<IFurniMaticPrize>.Empty;
		}

		ImmutableWeightedTable<IFurniMaticPrize>.Builder builder = ImmutableWeightedTable.CreateBuilder<IFurniMaticPrize>();

		List<IFurniMaticPrize> commonPrizes = new();
		double commonPrizeChance = 1;

		foreach (IFurniMaticPrizeLevel level in levels)
		{
			switch (level.Odds)
			{
				case <= 0:
					throw new ArgumentOutOfRangeException(nameof(levels), "Prize level odds must be positive!");
				case 1: //Combine all levels with 1 odds
					commonPrizes.AddRange(level.Prizes);
					continue;
			}

			double levelChance = 1.0 / level.Odds;
			double prizeChance = levelChance / level.Prizes.Length;

			foreach (IFurniMaticPrize prize in level.Prizes)
			{
				builder.Add(prize, prizeChance);
			}

			commonPrizeChance -= levelChance;
		}

		if (commonPrizes.Count <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(levels), "You must have at least one common prize!");
		}

		if (commonPrizeChance > 0)
		{
			double prizeChance = commonPrizeChance / commonPrizes.Count;

			foreach (IFurniMaticPrize prize in commonPrizes)
			{
				builder.Add(prize, prizeChance);
			}
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(levels), "The combined odds are too high! This makes ZERO sense!");
		}

		return builder.ToImmutable();
	}

	public bool TryGetPrize(int prizeId, [NotNullWhen(true)] out IFurniMaticPrize? prize) => this.prizes.TryGetValue(prizeId, out prize);
}
