using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal class UnitEnterRoomTriggerFurniture(int id, FloorFurnitureType type, Point2D dimensions, double heigh) : WiredTriggerFurniture(id, type, dimensions, heigh), IUnitEnterRoomTriggerFurniture;
