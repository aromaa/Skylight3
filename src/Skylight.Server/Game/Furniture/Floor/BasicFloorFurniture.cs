﻿using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class BasicFloorFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double height, int stateCount) : FixedHeightMultiStateFloorFurniture(id, kind, dimensions, height, stateCount), IBasicFloorFurniture;
