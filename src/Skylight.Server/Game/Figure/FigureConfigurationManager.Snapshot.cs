using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal partial class FigureConfigurationManager
{
	public IFigureDataContainer Parse(ReadOnlySequence<byte> figure, FigureValidationOptions validationOptions = default) => this.Current.Parse(figure, validationOptions);
	public IFigureDataContainer Parse(ReadOnlySpan<char> figure, FigureValidationOptions validationOptions = default) => this.Current.Parse(figure, validationOptions);

	public bool TryGetColorPaletteColor(int id, [NotNullWhen(true)] out IFigureColorPaletteColor? colorPaletteColor) => this.Current.TryGetColorPaletteColor(id, out colorPaletteColor);
	public bool TryGetFigureSetType(int id, [NotNullWhen(true)] out IFigureSetType? figureSetType) => this.Current.TryGetFigureSetType(id, out figureSetType);
	public bool TryGetFigureSet(int id, [NotNullWhen(true)] out IFigureSet? figureSet) => this.Current.TryGetFigureSet(id, out figureSet);

	public bool TryGetFigureValidator(string name, FigureSex sex, [NotNullWhen(true)] out IFigureValidator? validator) => this.Current.TryGetFigureValidator(name, sex, out validator);

	private sealed class Snapshot : IFigureConfigurationSnapshot
	{
		private readonly Cache cache;

		private readonly FrozenDictionary<string, IFigureSetType>.AlternateLookup<ReadOnlySpan<char>> figureSetTypes;

		public Snapshot(Cache cache)
		{
			this.cache = cache;

			this.figureSetTypes = cache.FigureSetTypesByType.GetAlternateLookup<ReadOnlySpan<char>>();
		}

		public IFigureDataContainer Parse(ReadOnlySequence<byte> figure, FigureValidationOptions validationOptions = default) => this.Parse(Encoding.UTF8.GetString(figure), validationOptions);

		public IFigureDataContainer Parse(ReadOnlySpan<char> figure, FigureValidationOptions validationOptions = default)
		{
			Dictionary<IFigureSetType, FigureSetValue> figureSetValues = [];
			foreach (Range setTypeRange in figure.Split('.'))
			{
				ReadOnlySpan<char> setType = figure[setTypeRange];

				MemoryExtensions.SpanSplitEnumerator<char> setTypeEnumerator = setType.Split('-');
				if (!setTypeEnumerator.MoveNext()
					|| !this.figureSetTypes.TryGetValue(setType[setTypeEnumerator.Current], out IFigureSetType? figureSetType))
				{
					continue;
				}

				if (!setTypeEnumerator.MoveNext()
					|| !int.TryParse(setType[setTypeEnumerator.Current], out int figureSetId)
					|| !figureSetType.Sets.TryGetValue(figureSetId, out IFigureSet? figureSet))
				{
					continue;
				}
				else if ((validationOptions.Subject is not null && !figureSet.CanWear(validationOptions.Subject))
					|| validationOptions.Validator?.Validate(figureSet, validationOptions.Subject) == false)
				{
					continue;
				}

				ImmutableArray<IFigureColorPaletteColor> colors;
				if (figureSet.ColorLayers > 0)
				{
					if (!setTypeEnumerator.MoveNext()
						|| !int.TryParse(setType[setTypeEnumerator.Current], out int color))
					{
						continue;
					}

					ImmutableArray<IFigureColorPaletteColor>.Builder figureColors = ImmutableArray.CreateBuilder<IFigureColorPaletteColor>(figureSet.ColorLayers);
					for (int i = 0; i < figureSet.ColorLayers; i++)
					{
						if (i > 0 && setTypeEnumerator.MoveNext())
						{
							if (!int.TryParse(setType[setTypeEnumerator.Current], out color))
							{
								break;
							}
						}

						if (!figureSetType.ColorPalette.Colors.TryGetValue(color, out IFigureColorPaletteColor? figureColor))
						{
							break;
						}
						else if (validationOptions.Subject is not null && !figureColor.CanWear(validationOptions.Subject))
						{
							break;
						}

						figureColors.Add(figureColor);
					}

					if (figureColors.Count != figureSet.ColorLayers)
					{
						continue;
					}

					colors = figureColors.MoveToImmutable();
				}
				else
				{
					colors = [];
				}

				figureSetValues[figureSetType] = new FigureSetValue(figureSet, colors);
			}

			FigureDataContainer container = new(figureSetValues.ToFrozenDictionary());
			if (validationOptions.Validator is not null)
			{
				HashSet<IFigureSetType> invalidTypes = validationOptions.Validator.Validate(container, validationOptions.Subject);
				if (invalidTypes.Count > 0)
				{
					foreach (IFigureSetType figureSetType in invalidTypes)
					{
						IFigureSet set = figureSetType.Sets.Values.First();

						figureSetValues[figureSetType] = new FigureSetValue(set, [.. Enumerable.Repeat(figureSetType.ColorPalette.Colors.Values.First(), set.ColorLayers)]);
					}

					container = new FigureDataContainer(figureSetValues.ToFrozenDictionary());
				}
			}

			return container;
		}

		public bool TryGetColorPaletteColor(int id, [NotNullWhen(true)] out IFigureColorPaletteColor? colorPaletteColor) => this.cache.FigureColorPaletteColors.TryGetValue(id, out colorPaletteColor);
		public bool TryGetFigureSetType(int id, [NotNullWhen(true)] out IFigureSetType? figureSetType) => this.cache.FigureSetTypesById.TryGetValue(id, out figureSetType);
		public bool TryGetFigureSet(int id, [NotNullWhen(true)] out IFigureSet? figureSet) => this.cache.FigureSets.TryGetValue(id, out figureSet);

		public bool TryGetFigureValidator(string name, FigureSex sex, [NotNullWhen(true)] out IFigureValidator? validator) => this.cache.FigureValidators.TryGetValue((name, sex), out validator);
	}
}
