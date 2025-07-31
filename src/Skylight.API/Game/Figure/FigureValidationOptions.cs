using Skylight.API.Game.Permissions;

namespace Skylight.API.Game.Figure;

public record struct FigureValidationOptions(IFigureValidator? Validator, IPermissionSubject? Subject);
