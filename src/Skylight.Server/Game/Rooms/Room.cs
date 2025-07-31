using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using Net.Collections;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Layout;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Rooms.Scheduler;

namespace Skylight.Server.Game.Rooms;

internal abstract class Room : IRoom
{
	public abstract IRoomInfo Info { get; }
	public abstract IRoomMap Map { get; }
	public abstract IRoomUnitManager UnitManager { get; }

	internal RoomTaskScheduler RoomTaskScheduler { get; }

	private SpinLock tickingLock; //Note: Mutating struct

	private readonly Queue<IRoomTask> scheduledUpdateTasks;

	private readonly SocketCollection roomClients;

	private readonly Thread thread;

	private bool active = true;

	protected Room(IRoomLayout roomLayout)
	{
		this.tickingLock = new SpinLock(enableThreadOwnerTracking: false);

		this.RoomTaskScheduler = new RoomTaskScheduler(this);

		this.scheduledUpdateTasks = new Queue<IRoomTask>();

		this.roomClients = new SocketCollection();

		this.thread = new Thread(this.DoTicking)
		{
			IsBackground = true
		};
	}

	internal ref SpinLock TickingLock => ref this.tickingLock;

	public int GameTime => 0;

	internal int UserCount => this.roomClients.Count;

	public abstract Task LoadAsync(CancellationToken cancellationToken);
	public void Start()
	{
		this.thread.Start();
	}

	public void Enter(IUser user)
	{
		this.roomClients.TryAdd(user.Client.Socket);

		//TODO: Abstract this better

		user.SendAsync(new RoomEntryTileOutgoingPacket(this.Map.Layout.DoorLocation.X, this.Map.Layout.DoorLocation.Y, this.Map.Layout.DoorDirection));
		user.SendAsync(new HeightMapOutgoingPacket
		{
			Width = this.Map.Layout.Size.X,
			HeightMap = Enumerable.Repeat(new TileHeightMap(1, false, true), this.Map.Layout.Size.X * this.Map.Layout.Size.Y).ToArray()
		});
		user.SendAsync(new FloorHeightMapOutgoingPacket
		{
			Scale = false,
			FixedWallsHeight = -1,
			HeightMap = this.Map.Layout.HeightMap
		});

		List<PublicRoomObjectData> publicRoomObjects = [];
		List<ObjectData<RoomItemId>> objects = [];
		List<ItemData<RoomItemId>> items = [];
		if (this is IPrivateRoom privateRoom)
		{
			foreach (IFloorRoomItem roomItem in privateRoom.ItemManager.FloorItems)
			{
				objects.Add(new ObjectData<RoomItemId>(roomItem.Id, roomItem.Furniture.Id, roomItem.Position.X, roomItem.Position.Y, roomItem.Position.Z, roomItem.Direction, roomItem.Height, 0, roomItem.GetItemData()));
			}

			foreach (IWallRoomItem roomItem in privateRoom.ItemManager.WallItems)
			{
				items.Add(new ItemData<RoomItemId>(roomItem.Id, roomItem.Furniture.Id, new WallPosition(roomItem.Location.X, roomItem.Location.Y, roomItem.Position.X, roomItem.Position.Y), roomItem.GetItemData()));
			}
		}

		//TODO: Public items
		user.SendAsync(new PublicRoomObjectsOutgoingPacket(this.Map.Layout.Id, publicRoomObjects));
		user.SendAsync(new ObjectsOutgoingPacket<RoomItemId>(objects, Array.Empty<(int, string)>()));
		user.SendAsync(new ItemsOutgoingPacket<RoomItemId>(items, Array.Empty<(int, string)>()));

		List<RoomUnitData> units = [];
		foreach (IUserRoomUnit unit in this.UnitManager.Units)
		{
			units.Add(new RoomUnitData
			{
				IdentifierId = unit.User.Profile.Id,
				Name = unit.User.Profile.Username,
				Motto = unit.User.Profile.Motto,
				Figure = unit.User.Profile.Avatar.Data.ToString(),
				RoomUnitId = unit.Id,
				X = unit.Position.X,
				Y = unit.Position.Y,
				Z = unit.Position.Z,
				Direction = unit.Rotation.X,
				Type = 1,
				Gender = unit.User.Profile.Avatar.Sex.ToNetwork(),
				GroupId = 0,
				GroupStatus = 0,
				GroupName = string.Empty,
				SwimSuit = string.Empty,
				AchievementScore = 666,
				IsModerator = true
			});
		}

		user.SendAsync(new UsersOutgoingPacket(units));

		if (this.Info is IPrivateRoomInfo privateRoomInfo)
		{
			IRoomCustomizationSettings customizationSettings = privateRoomInfo.Settings.CustomizationSettings;

			user.SendAsync(new RoomVisualizationSettingsOutgoingPacket(customizationSettings.HideWalls, customizationSettings.FloorThickness, customizationSettings.WallThickness));
		}

		user.SendAsync(new RoomEntryInfoOutgoingPacket(this.Info.Id, true));
	}

	public void Exit(IUser user)
	{
		this.roomClients.TryRemove(user.Client.Socket);
	}

	private void DoTicking()
	{
		const int TickTime = 500;

		while (this.active)
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

			this.DoTick();

			//After everything is done, run the tasks we received while ticking
			this.RoomTaskScheduler.ExecuteTasks();
		}
	}

	internal abstract void DoTick();

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

	public void Unload()
	{
		this.active = false;
	}
}
