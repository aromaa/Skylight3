using System.ComponentModel;

namespace Skylight.Server.Attributes;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class InternalProtocolLibraryPathAttribute : Attribute
{
	public string Path { get; }

	public InternalProtocolLibraryPathAttribute(string path)
	{
		this.Path = path;
	}
}
