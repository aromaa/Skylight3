using System.Diagnostics;
using Skylight.API.Collections.Cache;

namespace Skylight.Server.Game.Rooms;

internal abstract class RoomTypeManager
{
	private protected sealed class RoomTicket<TRoom, TInstance>(TInstance loadedRoom) : ICacheReference<TRoom>
		where TInstance : TicketTracked<TRoom>
	{
		private TInstance? loadedRoom = loadedRoom;

#if DEBUG
		~RoomTicket()
		{
			Debug.Assert(this.loadedRoom is null, "Ticket was not disposed");
		}
#endif

		public TRoom Value
		{
			get
			{
				ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);

				return this.loadedRoom.Room!;
			}
		}

		public ICacheReference<TRoom> Retain()
		{
			ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);
			ObjectDisposedException.ThrowIf(!this.loadedRoom.TryAcquireTicket(), this);

			return new RoomTicket<TRoom, TInstance>(this.loadedRoom);
		}

		public void Dispose()
		{
			ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);

			this.loadedRoom.ReleaseTicket();
			this.loadedRoom = null;
		}
	}

	private protected abstract class TicketTracked
	{
		private const uint Killed = 1u << 31;
		private const uint Unloading = 1u << 30;
		private const uint Mask = TicketTracked.Killed | TicketTracked.Unloading;

		//Bit 32 means we have unloaded, Bit 31 that we are performing unload.
		private volatile uint ticketsCount;

		internal bool TryAcquireTicket()
		{
			while (true)
			{
				uint ticketsCount = this.ticketsCount;
				if ((ticketsCount & TicketTracked.Killed) == 0)
				{
					//We are allowed the clear the unloading bit but NOT the killed.
					uint newTicketsCount = (ticketsCount & ~TicketTracked.Unloading) + 1;

					if (Interlocked.CompareExchange(ref this.ticketsCount, newTicketsCount, ticketsCount) == ticketsCount)
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}
		}

		internal void ReleaseTicket()
		{
			Debug.Assert(this.ticketsCount > 0, "Tickets count overflow");
			Debug.Assert((this.ticketsCount & TicketTracked.Mask) == 0, "Ticket released when unloading");

			if (Interlocked.Decrement(ref this.ticketsCount) == 0)
			{
				this.QueueUnload();
			}
		}

		protected abstract void QueueUnload();

		protected bool TryChangeStateToUnloading() => Interlocked.CompareExchange(ref this.ticketsCount, TicketTracked.Unloading, 0) == 0;
		protected bool TryChangeStateToKilled() => Interlocked.CompareExchange(ref this.ticketsCount, TicketTracked.Killed, TicketTracked.Unloading) == TicketTracked.Unloading;
	}

	private protected abstract class TicketTracked<T> : TicketTracked
	{
		internal abstract T? Room { get; }
	}
}
