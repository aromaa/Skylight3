using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal class UnitEnterRoomTriggerFurniture(int id, int width, int length, double height) : WiredTriggerFurniture(id, width, length, height), IUnitEnterRoomTriggerFurniture;
