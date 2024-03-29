﻿using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class BasicFloorFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IBasicFloorFurniture
{
	public override double DefaultHeight => height;
}
