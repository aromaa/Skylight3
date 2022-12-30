using Skylight.API.Game.Badges;

namespace Skylight.Server.Game.Badges;

internal sealed class Badge : IBadge
{
	public int Id { get; }

	public string Code { get; }

	internal Badge(int id, string code)
	{
		this.Id = id;

		this.Code = code;
	}
}
