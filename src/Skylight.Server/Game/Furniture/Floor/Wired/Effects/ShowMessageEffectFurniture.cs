using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal sealed class ShowMessageEffectFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double height) : WiredEffectFurniture(id, kind, dimensions, height), IShowMessageEffectFurniture;
