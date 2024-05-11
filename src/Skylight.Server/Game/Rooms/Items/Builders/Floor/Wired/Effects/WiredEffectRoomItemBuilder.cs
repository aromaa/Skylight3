using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Effect;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Effects;

internal abstract class WiredEffectRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder> : WiredRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>,
	IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>
	where TFurniture : IWiredEffectFurniture
	where TTarget : IWiredEffectRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : WiredEffectRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : WiredEffectRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
{
	protected int EffectDelayValue { get; set; } = -1;

	public IWiredEffectRoomItemDataBuilder EffectDelay(int effectDelay)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(effectDelay);

		this.EffectDelayValue = effectDelay;

		return this;
	}

	protected abstract TTarget Build(int effectDelay);

	public override TTarget Build()
	{
		int effectDelay = this.EffectDelayValue;
		if (effectDelay == -1)
		{
			if (this.ExtraDataValue is not null)
			{
				effectDelay = this.ExtraDataValue.RootElement.GetProperty("EffectDelay").GetInt32();
			}
			else
			{
				throw new InvalidOperationException("You must provide effect delay or extra data");
			}
		}

		return this.Build(effectDelay);
	}
}
