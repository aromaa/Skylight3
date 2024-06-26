﻿using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using Microsoft.EntityFrameworkCore;
using Net.Collections;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Server.Game.Rooms.GameMap;
using Skylight.Server.Game.Rooms.Items;
using Skylight.Server.Game.Rooms.Scheduler;
using Skylight.Server.Game.Rooms.Units;

namespace Skylight.Server.Game.Rooms;

internal sealed class Room : IRoom
{
	public IRoomInfo Info { get; }

	public IRoomMap Map { get; }
	public IRoomUnitManager UnitManager { get; }
	public IRoomItemManager ItemManager { get; }

	internal RoomTaskScheduler RoomTaskScheduler { get; }

	private SpinLock tickingLock; //Note: Mutating struct

	private readonly Queue<IRoomTask> scheduledUpdateTasks;

	private readonly SocketCollection roomClients;

	private readonly Thread thread;

	public Room(RoomData roomData, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IUserManager userManager, IRoomItemInteractionManager itemInteractionManager)
	{
		this.Info = roomData;

		this.tickingLock = new SpinLock(enableThreadOwnerTracking: false);

		this.RoomTaskScheduler = new RoomTaskScheduler(this);

		this.scheduledUpdateTasks = new Queue<IRoomTask>();

		this.Map = new RoomTileMap(this, roomData.Layout);
		this.UnitManager = new RoomUnitManager(this);
		this.ItemManager = new RoomItemManager(this, dbContextFactory, userManager, furnitureManager, floorRoomItemStrategy, wallRoomItemStrategy, itemInteractionManager);

		this.roomClients = new SocketCollection();

		this.thread = new Thread(this.DoTicking)
		{
			IsBackground = true
		};
		this.thread.Start();
	}

	internal ref SpinLock TickingLock => ref this.tickingLock;

	public int GameTime => 0;

	internal int UserCount => this.roomClients.Count;

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		await this.ItemManager.LoadAsync(cancellationToken).ConfigureAwait(false);
	}

	public void Enter(IUser user)
	{
		this.roomClients.TryAdd(user.Client.Socket);
	}

	public void Exit(IUser user)
	{
		this.roomClients.TryRemove(user.Client.Socket);
	}

	public bool IsOwner(IUser user) => this.Info.Owner.Id == user.Profile.Id;

	private void DoTicking()
	{
		const int TickTime = 500;

		while (true)
		{
			long startTime = Stopwatch.GetTimestamp();

			this.Tick();

			Thread.Sleep(Math.Max(TickTime, TickTime - Stopwatch.GetElapsedTime(startTime).Milliseconds));
		}
	}

	internal void Tick()
	{
		//Tick room inside the lock!
		using (this.tickingLock.Enter())
		{
			IRoomTask[] tasks;
			lock (this.scheduledUpdateTasks)
			{
				tasks = [.. this.scheduledUpdateTasks];

				this.scheduledUpdateTasks.Clear();
			}

			foreach (IRoomTask roomTask in tasks)
			{
				roomTask.Execute(this);
			}

			this.UnitManager.Tick();
			this.ItemManager.Tick();

			//After everything is done, run the tasks we received while ticking
			this.RoomTaskScheduler.ExecuteTasks();
		}
	}

	public bool PostTask<TTask>(TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.PostTask(task);

	public ValueTask PostTaskAsync<TTask>(TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.PostTaskAsync(task);

	public ValueTask<TResult> ScheduleTask<TTask, TResult>(TTask task)
		where TTask : IRoomTask<TResult> => this.RoomTaskScheduler.ScheduleTask<TTask, TResult>(task);

	public ValueTask<TResult> ScheduleTaskAsync<TTask, TResult>(TTask task)
		where TTask : IAsyncRoomTask<TResult> => this.RoomTaskScheduler.ScheduleTaskAsync<TTask, TResult>(task);

	public bool PostTask(Action<IRoom> action) => this.RoomTaskScheduler.PostTask(action);
	public ValueTask PostTaskAsync(Action<IRoom> action) => this.RoomTaskScheduler.PostTaskAsync(action);
	public ValueTask<TResult> ScheduleTask<TResult>(Func<IRoom, TResult> func) => this.RoomTaskScheduler.ScheduleTask(func);
	public ValueTask<TResult> ScheduleTaskAsync<TResult>(Func<IRoom, ValueTask<TResult>> func) => this.RoomTaskScheduler.ScheduleTaskAsync(func);

	public void ScheduleUpdateTask(IRoomTask task)
	{
		lock (this.scheduledUpdateTasks)
		{
			this.scheduledUpdateTasks.Enqueue(task);
		}
	}

	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
	{
		this.roomClients.SendAsync(packet);
	}
}
