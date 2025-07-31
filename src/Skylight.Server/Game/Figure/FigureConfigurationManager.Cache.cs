using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Permissions;
using Skylight.Domain.Figure;
using Skylight.Domain.Permissions;

namespace Skylight.Server.Game.Figure;

internal partial class FigureConfigurationManager
{
	private sealed class Cache
	{
		internal FrozenDictionary<int, IFigureColorPaletteColor> FigureColorPaletteColors { get; }
		internal FrozenDictionary<string, IFigureSetType> FigureSetTypesByType { get; }
		internal FrozenDictionary<int, IFigureSetType> FigureSetTypesById { get; }
		internal FrozenDictionary<int, IFigureSet> FigureSets { get; }
		internal FrozenDictionary<(string Name, FigureSex Sex), IFigureValidator> FigureValidators { get; }

		private Cache(FrozenDictionary<int, IFigureColorPaletteColor> figureColorPaletteColors, FrozenDictionary<string, IFigureSetType> figureSetTypesByType, FrozenDictionary<int, IFigureSetType> figureSetTypesById, FrozenDictionary<int, IFigureSet> figureSets, FrozenDictionary<(string Name, FigureSex Sex), IFigureValidator> figureValidators)
		{
			this.FigureColorPaletteColors = figureColorPaletteColors;
			this.FigureSetTypesByType = figureSetTypesByType;
			this.FigureSetTypesById = figureSetTypesById;
			this.FigureSets = figureSets;
			this.FigureValidators = figureValidators;
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<int, FigureColorPaletteEntity> palettes = [];
			private readonly Dictionary<int, FigureSetTypeEntity> figureTypeSets = [];
			private readonly Dictionary<int, FigureValidationEntity> figureValidations = [];

			public void AddPalette(FigureColorPaletteEntity paletteEntity)
			{
				this.palettes.Add(paletteEntity.Id, paletteEntity);
			}

			public void AddFigureTypeSet(FigureSetTypeEntity figureSetTypeEntity)
			{
				this.figureTypeSets.Add(figureSetTypeEntity.Id, figureSetTypeEntity);
			}

			public void AddFigureValidation(FigureValidationEntity figureValidationEntity)
			{
				this.figureValidations.Add(figureValidationEntity.Id, figureValidationEntity);
			}

			internal Cache ToImmutable() => new(FrozenDictionary<int, IFigureColorPaletteColor>.Empty, FrozenDictionary<string, IFigureSetType>.Empty, FrozenDictionary<int, IFigureSetType>.Empty, FrozenDictionary<int, IFigureSet>.Empty, FrozenDictionary<(string Name, FigureSex Sex), IFigureValidator>.Empty);

			internal async Task<Cache> ToImmutableAsync(IPermissionManager permissionManager, CancellationToken cancellationToken)
			{
				IPermissionDirectory<string> ranksDirectory = await permissionManager.GetRanksDirectoryAsync(cancellationToken).ConfigureAwait(false);

				Dictionary<int, IFigureColorPaletteColor> colorPaletteColors = [];
				Dictionary<int, IFigureColorPalette> colorPalettes = [];
				foreach (FigureColorPaletteEntity paletteEntity in this.palettes.Values)
				{
					Dictionary<int, IFigureColorPaletteColor> colors = [];
					foreach (FigureColorPaletteColorEntity figureColorEntity in paletteEntity.Colors!)
					{
						IPermissionSubject? permissionRequirement = null;
						if (figureColorEntity.RankId is { } rank)
						{
							permissionRequirement = await ranksDirectory.GetSubjectAsync(rank).ConfigureAwait(false);
							if (permissionRequirement is null)
							{
								throw new InvalidOperationException($"The color palette color {figureColorEntity.Id} is referring to non-existent rank {rank}!");
							}
						}

						ref IFigureColorPaletteColor? colorPaletteColor = ref CollectionsMarshal.GetValueRefOrAddDefault(colorPaletteColors, figureColorEntity.Id, out _);
						colorPaletteColor ??= new FigureColorPaletteColor(figureColorEntity.Id, permissionRequirement);

						colors.Add(figureColorEntity.Id, colorPaletteColor);
					}

					colorPalettes.Add(paletteEntity.Id, new FigureColorPalette(colors.ToFrozenDictionary()));
				}

				Dictionary<string, IFigureSetType> figureSetTypes = [];
				Dictionary<int, IFigureSetType> figureSetTypesById = [];
				Dictionary<int, IFigureSet> figureSets = [];
				foreach (FigureSetTypeEntity figureSetTypeEntity in this.figureTypeSets.Values)
				{
					if (!colorPalettes.TryGetValue(figureSetTypeEntity.ColorPaletteId, out IFigureColorPalette? palette))
					{
						throw new InvalidOperationException($"Figure palette with ID {figureSetTypeEntity.ColorPaletteId} not found for figure set type {figureSetTypeEntity.Id}.");
					}

					FigureSetType setType = new(figureSetTypeEntity.Id, figureSetTypeEntity.Type, palette);

					Dictionary<int, IFigureSet> sets = [];
					foreach (FigureSetEntity figureSetEntity in figureSetTypeEntity.Sets!)
					{
						IPermissionSubject? permissionRequirement = null;
						if (figureSetEntity.RankId is { } rank)
						{
							permissionRequirement = await ranksDirectory.GetSubjectAsync(rank).ConfigureAwait(false);
							if (permissionRequirement is null)
							{
								throw new InvalidOperationException($"The figure set {figureSetEntity.Id} is referring to non-existent rank {rank}!");
							}
						}

						FigureSex? sex = figureSetEntity.Sex switch
						{
							null => null,

							FigureSexType.Male => FigureSex.Male,
							FigureSexType.Female => FigureSex.Female,

							_ => throw new UnreachableException(),
						};

						ref IFigureSet? figureSet = ref CollectionsMarshal.GetValueRefOrAddDefault(figureSets, figureSetEntity.Id, out _);
						figureSet ??= new FigureSet(figureSetEntity.Id, setType, sex, permissionRequirement, figureSetEntity.Parts!.Max(e => e.ColorIndex));

						sets.Add(figureSetEntity.Id, figureSet);
					}

					setType.Init(sets.ToFrozenDictionary());

					figureSetTypes.Add(figureSetTypeEntity.Type, setType);
					figureSetTypesById.Add(figureSetTypeEntity.Id, setType);
				}

				Dictionary<(string, FigureSex), IFigureValidator> figureValidators = [];
				foreach (FigureValidationEntity figureValidationEntity in this.figureValidations.Values)
				{
					Dictionary<IFigureSetType, HashSet<IPermissionSubject>> figureSetTypeRules = [];
					foreach (FigureValidationSetTypeRuleEntity figureValidationSetTypeRuleEntity in figureValidationEntity.SetTypeRules!)
					{
						if (!figureSetTypesById.TryGetValue(figureValidationSetTypeRuleEntity.SetTypeId, out IFigureSetType? setType))
						{
							throw new InvalidOperationException($"Figure set type with ID {figureValidationSetTypeRuleEntity.SetTypeId} not found for figure validation {figureValidationEntity.Id}.");
						}

						HashSet<IPermissionSubject> exemptRanks = [];
						foreach (RankEntity exemptRankEntity in figureValidationSetTypeRuleEntity.ExemptRanks!)
						{
							IPermissionSubject? exemptRank = await ranksDirectory.GetSubjectAsync(exemptRankEntity.Id).ConfigureAwait(false);
							if (exemptRank is null)
							{
								throw new InvalidOperationException($"The figure validation {figureValidationEntity.Id} is referring to non-existent rank {exemptRankEntity.Id}!");
							}

							exemptRanks.Add(exemptRank);
						}

						figureSetTypeRules[setType] = exemptRanks;
					}

					FigureSex sex = figureValidationEntity.Sex switch
					{
						FigureSexType.Male => FigureSex.Male,
						FigureSexType.Female => FigureSex.Female,
						_ => throw new UnreachableException(),
					};

					figureValidators[(figureValidationEntity.Name, sex)] = new FigureValidator(figureSetTypeRules);
				}

				return new Cache(colorPaletteColors.ToFrozenDictionary(), figureSetTypes.ToFrozenDictionary(), figureSetTypesById.ToFrozenDictionary(), figureSets.ToFrozenDictionary(), figureValidators.ToFrozenDictionary());
			}
		}
	}
}
