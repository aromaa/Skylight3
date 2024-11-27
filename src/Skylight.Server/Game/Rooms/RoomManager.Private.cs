using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.Domain.Rooms.Layout;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms;

internal partial class RoomManager
{
	private sealed class LoadedPrivateRoom(RoomManager roomManager) : TicketTracked<IPrivateRoom>
	{
		private readonly RoomManager roomManager = roomManager;

		private object? value = new RoomLoadHandler();
		private ICacheReference<IPrivateRoomInfo>? roomInfo;

		internal override IPrivateRoom? Room
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? null
					: Unsafe.As<IPrivateRoom?>(value);
			}
		}

		internal ValueTask<IPrivateRoom> RoomTask
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? new ValueTask<IPrivateRoom>(Unsafe.As<RoomLoadHandler>(value).Task)
					: ValueTask.FromResult(Unsafe.As<IPrivateRoom>(value));
			}
		}

		internal Task LoadAsync(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue)
		{
			if (this.value is RoomLoadHandler loadHandler)
			{
				return loadHandler.LoadAsync(this, serviceProvider, dbContextFactory, roomInfoValue);
			}

			roomInfoValue.Dispose();

			return Task.CompletedTask;
		}

		protected override void QueueUnload() => this.roomManager.QueueUnload(this);

		internal void TryUnload()
		{
			if (!this.TryChangeStateToUnloading())
			{
				return;
			}

			IPrivateRoom room = (IPrivateRoom)this.value!;

			//TODO: Activate hybrid mode, stop ticking
			//TODO: Save

			if (!this.TryChangeStateToKilled())
			{
				return;
			}

			//Nobody requested access to the room while saving, we are now free to unload the room.

			this.value = null;

			this.roomManager.loadedPrivateRooms.TryRemove(KeyValuePair.Create(this.roomInfo!.Value.Id, this));

			room.Unload();

			this.roomInfo.Dispose();
		}

		private sealed class RoomLoadHandler
		{
			private readonly TaskCompletionSource<IPrivateRoom> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

			private volatile bool initialized;

			internal Task<IPrivateRoom> Task => this.taskCompletionSource.Task;

			internal Task LoadAsync(LoadedPrivateRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue, CancellationToken cancellationToken = default)
			{
				if (this.initialized || Interlocked.CompareExchange(ref this.initialized, true, false))
				{
					roomInfoValue.Dispose();

					return this.taskCompletionSource.Task;
				}

				return this.InternalLoadAsync(instance, serviceProvider, dbContextFactory, roomInfoValue, cancellationToken);
			}

			private async Task InternalLoadAsync(LoadedPrivateRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue, CancellationToken cancellationToken = default)
			{
				try
				{
					IPrivateRoomInfo roomInfo = roomInfoValue.Value;

					await using SkylightContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

					//TODO: Clean up
					CustomRoomLayoutEntity? customLayout = await dbContext.CustomRoomLayouts.FirstOrDefaultAsync(e => e.RoomId == roomInfo.Id, cancellationToken).ConfigureAwait(false);

					ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PrivateRoom),
					[
						typeof(IPrivateRoomInfo),
						typeof(IRoomLayout)
					]);

					PrivateRoom room = (PrivateRoom)roomFactory(serviceProvider,
					[
						roomInfo,
						customLayout is null ? roomInfo.Layout : new RoomLayout(roomInfo.Layout.Id, customLayout.HeightMap, customLayout.DoorX, customLayout.DoorY, customLayout.DoorDirection)
					]);

					await room.LoadAsync(cancellationToken).ConfigureAwait(false);

					room.Start();

					instance.value = room;
					instance.roomInfo = roomInfoValue;

					this.taskCompletionSource.SetResult(room);
				}
				catch (Exception ex)
				{
					roomInfoValue.Dispose();

					this.taskCompletionSource.SetException(ex);

					throw;
				}
			}
		}
	}
}
