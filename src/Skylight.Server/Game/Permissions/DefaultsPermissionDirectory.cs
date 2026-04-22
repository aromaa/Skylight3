using System.Collections.Concurrent;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class DefaultsPermissionDirectory : PermissionDirectory<string>
{
	private readonly ConcurrentDictionary<string, PermissionSubject<string>> subjects;

	public override IPermissionSubject Defaults { get; }

	internal DefaultsPermissionDirectory(PermissionManager manager, string id, Dictionary<string, PermissionContainer> seed)
		: base(manager, id)
	{
		this.subjects = [];
		foreach ((string identifer, PermissionContainer container) in seed)
		{
			this.subjects[identifer] = new PermissionSubject<string>(this, identifer, container);
		}

		if (!this.subjects.TryGetValue("defaults", out PermissionSubject<string>? defaults))
		{
			defaults = new PermissionSubject<string>(this, "defaults");
		}

		this.Defaults = defaults;
	}

	internal PermissionSubject<string> GetOrAddDefault(string identifier)
		=> this.subjects.GetOrAdd(identifier, i => new PermissionSubject<string>(this, i));

	public override ValueTask<IPermissionSubject?> GetSubjectAsync(string identifier) =>
		!this.subjects.TryGetValue(identifier, out PermissionSubject<string>? subject)
			? default
			: ValueTask.FromResult<IPermissionSubject?>(subject);
}
