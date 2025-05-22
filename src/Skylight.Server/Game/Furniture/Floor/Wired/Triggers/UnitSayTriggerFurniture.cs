using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal sealed class UnitSayTriggerFurniture(int id, FloorFurnitureKind kind, Point2D dimensions, double height) : WiredTriggerFurniture(id, kind, dimensions, height), IUnitSayTriggerFurniture;
