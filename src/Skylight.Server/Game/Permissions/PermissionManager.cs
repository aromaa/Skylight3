using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Permissions;
using Skylight.Domain.Permissions;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionManager(IDbContextFactory<SkylightContext> dbContextFactory) : IPermissionManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly TaskCompletionSource<Data> data = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public async Task LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			Dictionary<string, PermissionContainer> defaults = await LoadDefaultsAsync(dbContext, cancellationToken).ConfigureAwait(false);
			Dictionary<string, PermissionContainer> ranks = [];

			await foreach (RankEntity rankEntity in dbContext.Ranks
				.Include(e => e.Permissions)
				.Include(e => e.Entitlements)
				.Include(e => e.Children)
				.AsSplitQuery()
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				PermissionContainer rank = new();

				foreach (RankPermissionEntity permissionEntity in rankEntity.Permissions!)
				{
					rank.SetPermission(permissionEntity.Permission, permissionEntity.Value);
				}

				foreach (RankEntitlementEntity entitlementEntity in rankEntity.Entitlements!)
				{
					rank.SetEntitlement(entitlementEntity.Entitlement, entitlementEntity.Value);
				}

				foreach (RankChildEntity childEntity in rankEntity.Children!)
				{
					rank.AddParent(new PermissionSubjectReference<string>(this, "ranks", childEntity.ChildRankId));
				}

				ranks[rankEntity.Id] = rank;
			}

			DefaultsPermissionDirectory defaultsDirectory = new(this, "defaults", defaults);
			RanksPermissionDirectory ranksDirectory = new(this, "ranks", defaultsDirectory.GetOrAddDefault("ranks"), ranks);

			this.data.SetResult(new Data(this, defaultsDirectory, ranksDirectory));
		}

		async Task<Dictionary<string, PermissionContainer>> LoadDefaultsAsync(SkylightContext dbContext, CancellationToken cancellationToken)
		{
			Dictionary<string, PermissionContainer> containers = [];

			await foreach (PrincipalDefaultsPermissionEntity entity in dbContext.PrincipalDefaultsPermissions
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionContainer? container = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				container ??= new PermissionContainer();
				container.SetPermission(entity.Permission, entity.Value);
			}

			await foreach (PrincipalDefaultsEntitlementEntity entity in dbContext.PrincipalDefaultsEntitlements
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionContainer? container = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				container ??= new PermissionContainer();
				container.SetEntitlement(entity.Entitlement, entity.Value);
			}

			await foreach (PrincipalDefaultsRankEntity entity in dbContext.PrincipalDefaultsRanks
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				ref PermissionContainer? container = ref CollectionsMarshal.GetValueRefOrAddDefault(containers, entity.Identifier, out _);
				container ??= new PermissionContainer();
				container.AddParent(new PermissionSubjectReference<string>(this, "ranks", entity.RankId));
			}

			return containers;
		}
	}

	public async ValueTask<IPermissionSubject> GetDefaultsAsync(CancellationToken cancellationToken = default)
		=> (await this.GetDefaultsDirectoryAsync(cancellationToken).ConfigureAwait(false)).Defaults;

	public async ValueTask<IPermissionDirectory?> GetDirectoryAsync(string identifier, CancellationToken cancellationToken = default)
		=> identifier switch
		{
			"defaults" => await this.GetDefaultsDirectoryAsync(cancellationToken).ConfigureAwait(false),
			"ranks" => await this.GetRanksDirectoryAsync(cancellationToken).ConfigureAwait(false),
			"users" => await this.GetUserDirectoryAsync(cancellationToken).ConfigureAwait(false),

			_ => null
		};

	public async ValueTask<IPermissionDirectory<string>> GetDefaultsDirectoryAsync(CancellationToken cancellationToken = default)
		=> (await this.data.Task.ConfigureAwait(false)).Defaults;

	public async ValueTask<IPermissionDirectory<string>> GetRanksDirectoryAsync(CancellationToken cancellationToken = default)
		=> (await this.data.Task.ConfigureAwait(false)).Ranks;
	public async ValueTask<IPermissionDirectory<int>> GetUserDirectoryAsync(CancellationToken cancellationToken = default)
		=> (await this.data.Task.ConfigureAwait(false)).Users;

	private sealed class Data
	{
		private readonly PermissionManager permissionManager;

		internal DefaultsPermissionDirectory Defaults { get; }
		internal RanksPermissionDirectory Ranks { get; }
		internal AsyncPermissionDirectory<int> Users { get; }

		internal Data(PermissionManager permissionManager, DefaultsPermissionDirectory defaults, RanksPermissionDirectory ranks)
		{
			this.permissionManager = permissionManager;

			this.Defaults = defaults;
			this.Ranks = ranks;
			this.Users = new(permissionManager, "users", defaults.GetOrAddDefault("users"), this.LoadUser);
		}

		public async ValueTask<IPermissionSubject> GetDefaultsAsync(string identifier, CancellationToken cancellationToken = default)
			=> await this.Defaults.GetSubjectAsync(identifier).ConfigureAwait(false) ?? new PermissionSubject<string>(this.Defaults, identifier);

		private async ValueTask<IPermissionSubject?> LoadUser(int id)
		{
			await using SkylightContext dbContext = await this.permissionManager.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			PermissionSubject<int> user = new(this.Users, id);

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
				user.Container.AddParent(new PermissionSubjectReference<string>(this.permissionManager, "ranks", rankEntity.RankId));
			}

			return user;
		}
	}
}
