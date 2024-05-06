using Skylight.API.Game.Furniture.Floor.Wired.Effects;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal class TeleportUnitEffectFurniture(int id, int width, int length, double height) : WiredEffectFurniture(id, width, length, height), ITeleportUnitEffectFurniture;
