using System.Collections.Frozen;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Events;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Game.Figure;
using Skylight.Server.Scheduling;

namespace Skylight.Server.Game.Users;

internal sealed class UserInfo : IUserInfo
{
	private readonly DatabaseBackgroundWorker databaseBackgroundWorker;

	private readonly Lock mutationLock;

	private volatile UserInfoView view;

	public event EventHandler<ValueChangedEventArgs<FigureAvatar>>? AvatarChanged;
	public event EventHandler<ValueChangedEventArgs<string>>? MottoChanged;

	internal UserInfo(DatabaseBackgroundWorker databaseBackgroundWorker, UserEntity entity, IFigureConfigurationSnapshot figureConfigurationSnapshot)
	{
		this.databaseBackgroundWorker = databaseBackgroundWorker;

		this.mutationLock = new Lock();

		Dictionary<IFigureSetType, FigureSetValue> figureSets = [];
		foreach (UserFigureEntity userFigureSetEntity in entity.FigureSets!)
		{
			if (figureConfigurationSnapshot.TryGetFigureSet(userFigureSetEntity.SetId, out IFigureSet? set))
			{
				figureSets.Add(set.Type, new FigureSetValue(set, [.. userFigureSetEntity.Colors!.Select(id => set.Type.ColorPalette.Colors[id.ColorId])]));
			}
		}

		FigureAvatar avatar = new(entity.Sex == FigureSexType.Male ? FigureSex.Male : FigureSex.Female, new FigureDataContainer(figureSets.ToFrozenDictionary()));

		this.view = new UserInfoView(entity.Id, entity.Username, avatar, entity.Motto, entity.LastOnline);
	}

	public int Id => this.view.Id;
	public string Username => this.view.Username;

	public FigureAvatar Avatar
	{
		get => this.view.Avatar;
		set => this.Update(ref this.AvatarChanged, value, static v => v.Avatar, static (view, value) => new UserInfoView(view) { Avatar = value }, this.UpdateAvatar);
	}

	public string Motto
	{
		get => this.view.Motto;
		set => this.Update(ref this.MottoChanged, value, static v => v.Motto, static (view, value) => new UserInfoView(view) { Motto = value }, this.UpdateMotto);
	}

	public DateTime LastOnline
	{
		get => this.view.LastOnline;
		set => this.Transaction(() => this.view = new UserInfoView(this.view) { LastOnline = value });
	}

	public IUserInfoView Snapshot => this.view;

	public IUserInfoEvents Events(IUserInfoView view) => new EventsRegistration(this, view.Snapshot);

	private void UpdateMotto()
	{
		this.databaseBackgroundWorker.Submit(async (dbContextFactory, cancellationToken) =>
		{
			await using SkylightContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

			UserEntity entity = new()
			{
				Id = this.Id
			};

			dbContext.Users.Attach(entity);

			entity.Motto = this.view.Motto;

			await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		});
	}

	private void UpdateAvatar()
	{
		this.databaseBackgroundWorker.Submit(async (dbContextFactory, cancellationToken) =>
		{
			// TODO: Retry on deadlock?
			await using SkylightContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
			await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

			await dbContext.UserFigureSets
				.ToLinqToDBTable()
				.Merge()
				.Using(this.Avatar.Data.Sets.Select(static e => (SetTypeId: e.Key.Id, SetId: e.Value.Set.Id)))
				.On((e, f) => e.UserId == this.Id && e.SetTypeId == f.SetTypeId)
				.UpdateWhenMatched(static (e, f) => new UserFigureEntity { SetId = f.SetId })
				.InsertWhenNotMatched(f => new UserFigureEntity { UserId = this.Id, SetTypeId = f.SetTypeId, SetId = f.SetId })
				.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == this.Id)
				.MergeAsync(cancellationToken)
				.ConfigureAwait(false);

			await dbContext.UserFigureColors
				.ToLinqToDBTable()
				.Merge()
				.Using(this.Avatar.Data.Sets.SelectMany(static e => e.Value.Colors.Select(static (v, i) => (Index: i, Value: v)),
					(p, v) => (SetTypeId: p.Key.Id, Index: v.Index, ColorId: v.Value.Id)))
				.On((e, f) => e.UserId == this.Id && e.SetTypeId == f.SetTypeId && e.Index == f.Index)
				.UpdateWhenMatched(static (e, f) => new UserFigureColorEntity { ColorId = f.ColorId })
				.InsertWhenNotMatched(f => new UserFigureColorEntity { UserId = this.Id, SetTypeId = f.SetTypeId, Index = f.Index, ColorId = f.ColorId })
				.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == this.Id)
				.MergeAsync(cancellationToken)
				.ConfigureAwait(false);

			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
		});
	}

	private void Transaction(Action action)
	{
		lock (this.mutationLock)
		{
			action();
		}
	}

	private void Update<T>(ref EventHandler<ValueChangedEventArgs<T>>? handlers, T value, Func<UserInfoView, T> getter, Func<UserInfoView, T, UserInfoView> constructor, Action update)
		where T : IEquatable<T>
	{
		lock (this.mutationLock)
		{
			UserInfoView view = this.view;
			if (value.Equals(getter(view)))
			{
				return;
			}

			this.view = constructor(view, value);

			update();

			handlers?.Invoke(this, new ValueChangedEventArgs<T>(getter(view), getter(this.view)));
		}
	}

	private sealed class EventsRegistration(UserInfo info, IUserInfoView view) : IUserInfoEvents
	{
		private readonly UserInfo info = info;
		private readonly IUserInfoView view = view;

		public event EventHandler<ValueChangedEventArgs<FigureAvatar>> AvatarChanged
		{
			add => this.Add(ref this.info.AvatarChanged, value, static v => v.Avatar);
			remove => this.Remove(ref this.info.AvatarChanged, value, static v => v.Avatar);
		}

		public event EventHandler<ValueChangedEventArgs<string>> MottoChanged
		{
			add => this.Add(ref this.info.MottoChanged, value, static v => v.Motto);
			remove => this.Remove(ref this.info.MottoChanged, value, static v => v.Motto);
		}

		private void Add<T>(ref EventHandler<ValueChangedEventArgs<T>>? handlers, EventHandler<ValueChangedEventArgs<T>> newHandler, Func<IUserInfoView, T> getter)
			where T : IEquatable<T>
		{
			lock (this.info.mutationLock)
			{
				handlers += newHandler;

				this.CallEvent(newHandler, getter);
			}
		}

		private void Remove<T>(ref EventHandler<ValueChangedEventArgs<T>>? handlers, EventHandler<ValueChangedEventArgs<T>> handler, Func<IUserInfoView, T> getter)
			where T : IEquatable<T>
		{
			lock (this.info.mutationLock)
			{
				handlers -= handler;

				this.CallEvent(handler, getter);
			}
		}

		private void CallEvent<T>(EventHandler<ValueChangedEventArgs<T>> handler, Func<IUserInfoView, T> getter)
			where T : IEquatable<T>
		{
			IUserInfoView oldView = this.view;
			UserInfoView currentView = this.info.view;
			if (oldView != currentView)
			{
				T oldValue = getter(oldView);
				T newValue = getter(currentView);
				if (!oldValue.Equals(newValue))
				{
					handler(this.info, new ValueChangedEventArgs<T>(oldValue, newValue));
				}
			}
		}
	}
}
