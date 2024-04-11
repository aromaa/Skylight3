using System.Diagnostics;

namespace Skylight.Bootstrap.Attributes;

[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class InternalProtocolLibraryPathAttribute(string path) : Attribute
{
	internal string Path { get; } = path;
}
