﻿using System.Collections.Frozen;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Numerics;
using Skylight.Domain.Furniture;
using Skylight.Server.Game.Furniture.Floor;
using Skylight.Server.Game.Furniture.Floor.Wired.Effects;
using Skylight.Server.Game.Furniture.Floor.Wired.Triggers;
using Skylight.Server.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture;

internal partial class FurnitureManager
{
	private sealed class Cache
	{
		internal FrozenDictionary<int, IFloorFurniture> FloorFurnitures { get; }
		internal FrozenDictionary<int, IWallFurniture> WallFurnitures { get; }

		internal Cache(Dictionary<int, IFloorFurniture> floorFurnitures, Dictionary<int, IWallFurniture> wallFurnitures)
		{
			this.FloorFurnitures = floorFurnitures.ToFrozenDictionary();
			this.WallFurnitures = wallFurnitures.ToFrozenDictionary();
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<int, FloorFurnitureEntity> floorFurnitures;
			private readonly Dictionary<int, WallFurnitureEntity> wallFurnitures;

			internal Builder()
			{
				this.floorFurnitures = [];
				this.wallFurnitures = [];
			}

			internal void AddFloorItem(FloorFurnitureEntity floorItem)
			{
				this.floorFurnitures.Add(floorItem.Id, floorItem);
			}

			internal void AddWallItem(WallFurnitureEntity wallItem)
			{
				this.wallFurnitures.Add(wallItem.Id, wallItem);
			}

			internal Cache ToImmutable()
			{
				Dictionary<int, IFloorFurniture> floorFurnitures = [];
				Dictionary<int, IWallFurniture> wallFurnitures = [];

				foreach (FloorFurnitureEntity entity in this.floorFurnitures.Values)
				{
					FloorFurnitureKind kind = entity.Kind switch
					{
						"walkable" => FloorFurnitureKind.Walkable,
						"seat" => FloorFurnitureKind.Seat,
						"bed" => FloorFurnitureKind.Bed,

						_ => FloorFurnitureKind.Obstacle
					};

					Point2D dimensions = new(entity.Width, entity.Length);

					FloorFurniture item = entity.InteractionType switch
					{
						//Todo: Factory
						"sticky_note_pole" => new StickyNotePoleFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"furnimatic_gift" => new FurniMaticGiftFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"sound_machine" => new SoundMachineFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"sound_set" => CreateSoundSet(entity, kind, dimensions),
						"roller" => new RollerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"basic" => new BasicFloorFurniture(entity.Id, kind, dimensions, entity.Height[0], int.Parse(entity.InteractionData)),
						"wired_on_say" => new UnitSayTriggerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_show_message" => new ShowMessageEffectFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_unit_enter_room" => new UnitEnterRoomTriggerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_unit_use_item" => new UnitUseItemTriggerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_cycle_item_state" => new CycleItemStateEffectFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_teleport_unit" => new TeleportUnitEffectFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_unit_walk_on" => new UnitWalkOnTriggerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"wired_unit_walk_off" => new UnitWalkOffTriggerFurniture(entity.Id, kind, dimensions, entity.Height[0]),
						"variable_height" => new VariableHeightFurniture(entity.Id, kind, dimensions, [.. entity.Height], int.Parse(entity.InteractionData)),

						_ => new StaticFloorFurniture(entity.Id, kind, dimensions, entity.Height[0])
					};

					floorFurnitures.Add(item.Id, item);

					static SoundSetFurniture CreateSoundSet(FloorFurnitureEntity entity, FloorFurnitureKind kind, Point2D dimensions)
					{
						int soundSetId = int.Parse(entity.ClassName.AsSpan(entity.ClassName.LastIndexOf('_') + 1));

						return new SoundSetFurniture(entity.Id, kind, dimensions, entity.Height[0], soundSetId, Enumerable.Range((soundSetId * 9) - 8, 9).ToFrozenSet());
					}
				}

				foreach (WallFurnitureEntity entity in this.wallFurnitures.Values)
				{
					WallFurniture item = entity.InteractionType switch
					{
						//Todo: Factory
						"sticky_note" => new StickyNoteFurniture(entity.Id),

						_ => new StaticWallFurniture(entity.Id)
					};

					wallFurnitures.Add(item.Id, item);
				}

				return new Cache(floorFurnitures, wallFurnitures);
			}
		}
	}
}
