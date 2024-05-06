using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal sealed class UnitWalkOffTriggerFurniture(int id, int width, int length, double height) : WiredTriggerFurniture(id, width, length, height), IUnitWalkOffTriggerFurniture;
