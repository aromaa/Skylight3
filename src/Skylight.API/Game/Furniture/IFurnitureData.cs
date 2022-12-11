using System.Text.Json;

namespace Skylight.API.Game.Furniture;

//TODO: Rethink how we are actually going to do this
public interface IFurnitureData<T>
{
	public JsonDocument? AsExtraData();
}
