using System.Collections.Frozen;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;
using Skylight.Server.Game.Figure;

namespace Skylight.Server.Game.Users;

internal sealed class UserInfo : IUserProfile
{
	public int Id { get; }

	public string Username { get; set; }
	public FigureAvatar Avatar { get; set; }
	public string Motto { get; set; }

	public DateTime LastOnline { get; set; }

	internal UserInfo(UserEntity entity, IFigureConfigurationSnapshot figureConfigurationSnapshot)
	{
		this.Id = entity.Id;

		Dictionary<IFigureSetType, FigureSetValue> figureSets = [];
		foreach (UserFigureEntity userFigureSetEntity in entity.FigureSets!)
		{
			if (figureConfigurationSnapshot.TryGetFigureSet(userFigureSetEntity.SetId, out IFigureSet? set))
			{
				figureSets.Add(set.Type, new FigureSetValue(set, [.. userFigureSetEntity.Colors!.Select(id => set.Type.ColorPalette.Colors[id.ColorId])]));
			}
		}

		this.Username = entity.Username;
		this.Avatar = new FigureAvatar(entity.Sex == FigureSexType.Male ? FigureSex.Male : FigureSex.Female, new FigureDataContainer(figureSets.ToFrozenDictionary()));
		this.Motto = entity.Motto;

		this.LastOnline = entity.LastOnline;
	}
}
