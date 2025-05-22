using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal class TeleportUnitEffectFurniture(int id, FloorFurnitureKind kind, Point2D dimensions, double height) : WiredEffectFurniture(id, kind, dimensions, height), ITeleportUnitEffectFurniture;
