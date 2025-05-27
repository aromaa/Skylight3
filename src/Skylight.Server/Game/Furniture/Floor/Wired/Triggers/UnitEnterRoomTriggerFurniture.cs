using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal class UnitEnterRoomTriggerFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double heigh) : WiredTriggerFurniture(id, kind, dimensions, heigh), IUnitEnterRoomTriggerFurniture;
