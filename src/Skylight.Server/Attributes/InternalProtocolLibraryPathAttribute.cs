using System.ComponentModel;

namespace Skylight.Server.Attributes;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class InternalProtocolLibraryPathAttribute(string path) : Attribute
{
	public string Path { get; } = path;
}
