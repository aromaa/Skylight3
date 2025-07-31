using System.Diagnostics;
using Skylight.API.Game.Figure;

namespace Skylight.Server.Extensions;

internal static class FigureSexExtensions
{
	internal static string ToNetwork(this FigureSex sex)
		=> sex switch
		{
			FigureSex.Male => "M",
			FigureSex.Female => "F",

			_ => throw new UnreachableException()
		};
}
