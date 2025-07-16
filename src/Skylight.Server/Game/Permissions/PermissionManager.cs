using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Permissions;
using Skylight.Domain.Permissions;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionManager : IPermissionManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private PermissionDirectory<string> defaults;
	private PermissionDirectory<string> ranks;
	private PermissionDirectory<int> users;

	public PermissionManager(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
		this.defaults = new PermissionDirectory<string>(this, "defaults", null, i => default);
		this.ranks = new PermissionDirectory<string>(this, "ranks", null, i => default);
		this.users = new PermissionDirectory<int>(this, "users", null, i => default);
	}

	public IPermissionSubject Defaults => this.defaults.Defaults;

	public async Task LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		PermissionDirectory<string> defaults;
		PermissionDirectory<string> ranks;
		PermissionDirectory<int> users;

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			defaults = await LoadDefaultsAsync(dbContext, cancellationToken).ConfigureAwait(false);

			IPermissionSubject ranksDefaults = (await defaults.GetSubjectAsync("ranks").ConfigureAwait(false)) ?? new PermissionSubject<string>(defaults, "ranks");
			IPermissionSubject usersDefaults = (await defaults.GetSubjectAsync("users").ConfigureAwait(false)) ?? new PermissionSubject<string>(defaults, "users");

			Dictionary<string, PermissionSubject<string>> cachedRanks = [];

			ranks = new PermissionDirectory<string>(this, "ranks", () => ranksDefaults, i => !cachedRanks.TryGetValue(i, out PermissionSubject<string>? value)
				? default
				: ValueTask.FromResult<IPermissionSubject?>(value));

			await foreach (RankEntity rankEntity in dbContext.Ranks
				.Include(e => e.Permissions)
				.Include(e => e.Entitlements)
				.Include(e => e.Children)
				.AsSplitQuery()
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				PermissionSubject<string> rank = new(ranks, rankEntity.Id);

				foreach (RankPermissionEntity permissionEntity in rankEntity.Permissions!)
				{
					rank.Container.SetPermission(permissionEntity.Permission, permissionEntity.Value);
				}

				foreach (RankEntitlementEntity entitlementEntity in rankEntity.Entitlements!)
				{
					rank.Container.SetEntitlement(entitlementEntity.Entitlement, entitlementEntity.Value);
				}

				foreach (RankChildEntity childEntity in rankEntity.Children!)
				{
					rank.Container.AddParent(new PermissionSubjectReference<string>(this, "ranks", childEntity.ChildRankId));
				}

				cachedRanks[rankEntity.Id] = rank;
			}

			users = new PermissionDirectory<int>(this, "users", () => usersDefaults, this.LoadUser);
		}

		context.Commit(() =>
		{
			this.defaults = defaults;
			this.ranks = ranks;
			this.users = users;
		});

		async Task<PermissionDirectory<string>> LoadDefaultsAsync(SkylightContext dbContext, CancellationToken cancellationToken)
		{
			Dictionary<string, PermissionSubject<string>> containers = [];

			PermissionDirectory<string> directory = new(this, "defaults", () => containers["defaults"], i => !containers.TryGetValue(i, out PermissionSubject<string>? value)
				? default
				: ValueTask.FromResult<IPermissionSubject?>(value));

			await foreach (PrincipalDefaultsPermissionEntity entity in dbContext.PrincipalDefaultsPermissions
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionSubject<string>? subject = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				subject ??= new PermissionSubject<string>(directory, entity.Identifier);
				subject.Container.SetPermission(entity.Permission, entity.Value);
			}

			await foreach (PrincipalDefaultsEntitlementEntity entity in dbContext.PrincipalDefaultsEntitlements
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionSubject<string>? container = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				container ??= new PermissionSubject<string>(directory, entity.Identifier);
				container.Container.SetEntitlement(entity.Entitlement, entity.Value);
			}

			await foreach (PrincipalDefaultsRankEntity entity in dbContext.PrincipalDefaultsRanks
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionSubject<string>? container = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				container ??= new PermissionSubject<string>(directory, entity.Identifier);
				container.Container.AddParent(new PermissionSubjectReference<string>(this, "ranks", entity.RankId));
			}

			if (!containers.ContainsKey("defaults"))
			{
				containers["defaults"] = new PermissionSubject<string>(directory, "defaults");
			}

			return directory;
		}
	}

	public async ValueTask<IPermissionDirectory?> GetDirectoryAsync(string identifier, CancellationToken cancellationToken = default)
		=> identifier switch
		{
			"defaults" => await this.GetDefaultsDirectoryAsync(cancellationToken).ConfigureAwait(false),
			"ranks" => await this.GetRanksDirectoryAsync(cancellationToken).ConfigureAwait(false),
			"users" => await this.GetUserDirectoryAsync(cancellationToken).ConfigureAwait(false),

			_ => null
		};

	public ValueTask<IPermissionDirectory<string>> GetDefaultsDirectoryAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult<IPermissionDirectory<string>>(this.defaults);
	public ValueTask<IPermissionDirectory<string>> GetRanksDirectoryAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult<IPermissionDirectory<string>>(this.ranks);
	public ValueTask<IPermissionDirectory<int>> GetUserDirectoryAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult<IPermissionDirectory<int>>(this.users);

	private async ValueTask<IPermissionSubject?> LoadUser(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		PermissionSubject<int> user = new(this.users, id);

		await foreach (UserPermissionEntity permissionEntity in dbContext.UserPermissions
			.Where(e => e.UserId == id)
			.AsAsyncEnumerable()
			.ConfigureAwait(false))
		{
			user.Container.SetPermission(permissionEntity.Permission, permissionEntity.Value);
		}

		await foreach (UserEntitlementEntity entitlementEntity in dbContext.UserEntitlements
			.Where(e => e.UserId == id)
			.AsAsyncEnumerable()
			.ConfigureAwait(false))
		{
			user.Container.SetEntitlement(entitlementEntity.Entitlement, entitlementEntity.Value);
		}

		await foreach (UserRankEntity rankEntity in dbContext.UserRanks
			.Where(e => e.UserId == id)
			.AsAsyncEnumerable()
			.ConfigureAwait(false))
		{
			user.Container.AddParent(new PermissionSubjectReference<string>(this, "ranks", rankEntity.RankId));
		}

		return user;
	}
}
