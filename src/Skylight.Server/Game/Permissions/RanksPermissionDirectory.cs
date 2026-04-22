using System.Collections.Concurrent;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class RanksPermissionDirectory : PermissionDirectory<string>
{
	private readonly ConcurrentDictionary<string, PermissionSubject<string>> subjects;

	public override IPermissionSubject Defaults { get; }

	internal RanksPermissionDirectory(PermissionManager manager, string id, IPermissionSubject defaults, Dictionary<string, PermissionContainer> seed)
		: base(manager, id)
	{
		this.subjects = [];
		foreach ((string identifer, PermissionContainer container) in seed)
		{
			this.subjects[identifer] = new PermissionSubject<string>(this, identifer, container);
		}

		this.Defaults = defaults;
	}

	public override ValueTask<IPermissionSubject?> GetSubjectAsync(string identifier) =>
		!this.subjects.TryGetValue(identifier, out PermissionSubject<string>? subject)
			? default
			: ValueTask.FromResult<IPermissionSubject?>(subject);
}
