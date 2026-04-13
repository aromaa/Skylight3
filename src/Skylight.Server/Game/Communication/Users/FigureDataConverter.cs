using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Figure;
using Skylight.Protocol.Packets.Convertors.Users;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class FigureDataConverter : IFigureDataConverter<IFigureDataContainer>
{
	public static string Mobiles(IFigureDataContainer value)
	{
		// TODO: Might just allow user to specify this themselves
		// or alternatively we could do mapping based solely on color.

		return "3,3,3";
	}

	public static string Goldfish(IFigureDataContainer value)
	{
		StringBuilder stringBuilder = new();

		HashSet<string> parts = [];
		foreach (FigureSetValue setValue in value.Sets.Values)
		{
			foreach (IFigureSetPart part in setValue.Set.Parts)
			{
				parts.Add(part.Part.PartType.Type);

				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('&');
				}

				stringBuilder.Append(part.Part.PartType.Type);
				stringBuilder.Append('=');
				if (int.TryParse(part.Part.Key, out int partId))
				{
					stringBuilder.Append($"{partId:000}");
				}
				else
				{
					stringBuilder.Append(part.Part.Key);
				}

				foreach (IFigureColorPaletteColor color in setValue.Colors)
				{
					stringBuilder.Append('/');
					stringBuilder.Append(color.Color.R);
					stringBuilder.Append(',');
					stringBuilder.Append(color.Color.G);
					stringBuilder.Append(',');
					stringBuilder.Append(color.Color.B);
				}
			}
		}

		// Client will crash if these are not present
		AddRequired("bd");
		AddRequired("rh");
		AddRequired("sh");
		AddRequired("hd");
		AddRequired("fc");
		AddRequired("ey");
		AddRequired("hr");
		AddRequired("lh");
		AddRequired("lg");
		AddRequired("ch");
		AddRequired("rs");
		AddRequired("ls");

		return stringBuilder.ToString();

		void AddRequired(string key)
		{
			if (!parts.Contains(key))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('&');
				}

				stringBuilder.Append(key);
				stringBuilder.Append('=');
				stringBuilder.Append(0);
			}
		}
	}

	public static string Classic(IFigureDataContainer value)
	{
		// This is the worst format the client ever supported.
		// Its hardcoded set of 5 items consisting of two pairs
		// of set id (3 chars) and color id (2 chars).
		// It must consist of only numerals and be excatly
		// the size of 25. Zero variation allowed.

		return string.Create(25, value.Sets, (span, sets) =>
		{
			span.Fill('0');

			foreach ((IFigureSetType setType, FigureSetValue setValue) in value.Sets)
			{
				int index = setType.Type switch
				{
					"hr" => 0,
					"hd" => 1,
					"lg" => 2,
					"sh" => 3,
					"ch" => 4,
					_ => -1
				};

				if (index >= 0)
				{
					int.Min(setValue.Set.Id, 999).TryFormat(span.Slice(index * 5), out _, "000");

					if (!setValue.Colors.IsEmpty)
					{
						int.Min(setValue.Colors[0].Id, 99).TryFormat(span.Slice((index * 5) + 3), out _, "00");
					}
				}
			}
		});
	}

	public static string Modern(IFigureDataContainer value) => value.ToString();
}
