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

	public Room(RoomData roomData, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IRoomItemInteractionManager itemInteractionManager, IUserManager userManager)
	{
		this.Info = roomData;

		this.tickingLock = new SpinLock(enableThreadOwnerTracking: false);

		this.RoomTaskScheduler = new RoomTaskScheduler(this);

		this.scheduledUpdateTasks = new Queue<IRoomTask>();

		this.Map = new RoomTileMap(this, roomData.Layout);
		this.UnitManager = new RoomUnitManager(this);
		this.ItemManager = new RoomItemManager(this, dbContextFactory, furnitureManager, floorRoomItemStrategy, wallRoomItemStrategy, itemInteractionManager, userManager);

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
				tasks = this.scheduledUpdateTasks.ToArray();

				this.scheduledUpdateTasks.Clear();
			}

			foreach (IRoomTask roomTask in tasks)
			{
				roomTask.Execute(this);
			}

			this.UnitManager.Tick();

			//After everything is done, run the tasks we received while ticking
			this.RoomTaskScheduler.ExecuteTasks();
		}
	}

	public bool ScheduleTask<TTask>(in TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.ScheduleTask(task);
	public bool ScheduleTask(Action<IRoom> action) => this.RoomTaskScheduler.ScheduleTask(action);
	public bool ScheduleTask<TState>(Action<IRoom, TState> action, in TState state) => this.RoomTaskScheduler.ScheduleTask(action, state);
	public ValueTask ScheduleTaskAsync<TTask>(in TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.ScheduleTaskAsync(task);
	public ValueTask ScheduleTaskAsync(Action<IRoom> action) => this.RoomTaskScheduler.ScheduleTaskAsync(action);
	public ValueTask ScheduleTaskAsync<TState>(Action<IRoom, TState> action, in TState state) => this.RoomTaskScheduler.ScheduleTaskAsync(action, state);
	public ValueTask ScheduleTaskAsync(Func<IRoom, ValueTask> func) => this.RoomTaskScheduler.ScheduleTaskAsync(func);
	public ValueTask ScheduleTaskAsync<TState>(Func<IRoom, TState, ValueTask> func, in TState state) => this.RoomTaskScheduler.ScheduleTaskAsync(func, state);
	public ValueTask<TOut> ScheduleTaskAsync<TOut>(Func<IRoom, TOut> func) => this.RoomTaskScheduler.ScheduleTaskAsync(func);
	public ValueTask<TOut> ScheduleTaskAsync<TState, TOut>(Func<IRoom, TState, TOut> func, in TState state) => this.RoomTaskScheduler.ScheduleTaskAsync(func, state);
	public ValueTask<TOut> ScheduleTaskAsync<TOut>(Func<IRoom, ValueTask<TOut>> func) => this.RoomTaskScheduler.ScheduleTaskAsync(func);
	public ValueTask<TOut> ScheduleTaskAsync<TState, TOut>(Func<IRoom, TState, ValueTask<TOut>> func, in TState state) => this.RoomTaskScheduler.ScheduleTaskAsync(func, state);

	public void ScheduleUpdateTask(IRoomTask task)
	{
		lock (this.scheduledUpdateTasks)
		{
			this.scheduledUpdateTasks.Enqueue(task);
		}
	}

	public ValueTask SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
	{
		return new ValueTask(this.roomClients.SendAsync(packet));
	}
}
