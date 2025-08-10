using System.Linq.Expressions;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomActivity
{
	private readonly int[] weeks;
	private readonly int[] days;

	private int weekOffset;
	private int currentDay;
	private int currentScore;

	private RoomActivity(int[] weeks, int[] days, int weekOffset, int day)
	{
		this.weeks = weeks;
		this.days = days;

		this.weekOffset = weekOffset;

		this.Recompute(day);
	}

	internal int Score => this.currentScore;

	internal void Update(int day, int value)
	{
		if (this.currentDay != day)
		{
			Slow(day, value);
		}

		this.days[0] += value;
		this.currentScore += value;

		void Slow(int day, int value)
		{
			if (this.currentDay > day)
			{
				// The system clock went backwards, handle gracefully
				int offset = this.currentDay - day;
				if (offset < this.days.Length)
				{
					// Make up for rounding errors
					int score = RoomActivity.GetScore(offset, this.days[offset]);

					this.days[offset] += value;
					this.currentScore += RoomActivity.GetScore(offset, this.days[offset]) - score;
				}
				else
				{
					int weekIndex = (offset / 7) - 1;
					if (weekIndex < this.weeks.Length)
					{
						// Make up for rounding errors
						int score = RoomActivity.GetScore(offset, this.weeks[weekIndex]);

						this.weeks[weekIndex] += value;
						this.currentScore += RoomActivity.GetScore(offset, this.weeks[weekIndex]) - score;
					}
				}
			}
			else
			{
				// New day, move everything downwards
				// so that the current day is in index 0.
				int weekOffset = (day - this.weekOffset) / 7;
				if (weekOffset > 0)
				{
					for (int i = this.weeks.Length - 1; i >= 0; i--)
					{
						(int oldValue, this.weeks[i]) = (this.weeks[i], 0);
						if (oldValue == 0)
						{
							continue;
						}

						int target = i + weekOffset;
						if (target < this.weeks.Length)
						{
							this.weeks[target] = oldValue;
						}
					}

					this.weekOffset = day;
				}

				int dayOffset = day - this.currentDay;
				for (int i = this.days.Length - 1; i >= 0; i--)
				{
					(int oldValue, this.days[i]) = (this.days[i], 0);
					if (oldValue == 0)
					{
						continue;
					}

					int newIndex = i + dayOffset;
					if (newIndex < this.days.Length)
					{
						this.days[newIndex] = oldValue;
					}
					else
					{
						int weekIndex = (newIndex / 7) - 1;
						if (weekIndex < this.weeks.Length)
						{
							this.weeks[weekIndex] += oldValue;
						}
					}
				}

				this.Recompute(day);
			}
		}
	}

	private void Recompute(int day)
	{
		int score = 0;
		for (int i = 0; i < this.days.Length; i++)
		{
			score += RoomActivity.GetScore(i, this.days[i]);
		}

		for (int i = 0, j = 7; i < this.weeks.Length; i++, j += 7)
		{
			score += RoomActivity.GetScore(j, this.weeks[i]);
		}

		this.currentScore = score;
		this.currentDay = day;
	}

	internal static int GetScore(int day, int score) => day <= 0
		? score
		: (int)(score * (1 - ((double)day / (1 + day))));

	internal static Expression<Func<PrivateRoomActivityEntity, int>> GetScore(int day) => e => e.Day == day
		? e.Value
		: e.Day <= day - 7
			? (int)(e.Value * (1 - ((double)(((day - e.Day) / 7) * 7) / ((((day - e.Day) / 7) * 7) + 1))))
			: (int)(e.Value * (1 - ((double)(day - e.Day) / (day + 1 - e.Day))));

	internal static Expression<Func<PrivateRoomActivityEntity, int>> GetScore2(int day) => e => e.Day == day
		? e.Value
		: (int)(e.Value * (1 - ((double)(day - e.Day) / (day + 1 - e.Day))));

	internal static int GetDay(DateTimeOffset dateTime) => (int)(dateTime.ToUnixTimeSeconds() / TimeSpan.SecondsPerDay);

	internal static async Task<RoomActivity> LoadAsync(int maxDays, int maxWeeks, int day, IAsyncEnumerable<PrivateRoomActivityEntity> values)
	{
		int weekStart = day - 7;
		int weekOffset = day;
		int[] weeks = new int[maxWeeks];
		int[] days = new int[maxDays];
		await foreach (PrivateRoomActivityEntity value in values.ConfigureAwait(false))
		{
			int dayIndex = day - value.Day;
			if (dayIndex < days.Length)
			{
				days[dayIndex] += value.Value;
			}
			else
			{
				int relativeWeek = weekStart - value.Day;
				int weekIndex = relativeWeek / 7;
				if (weekIndex < weeks.Length)
				{
					weekOffset = int.Max(day - relativeWeek, day - 6);
					weeks[weekIndex] += value.Value;
				}
			}
		}

		return new RoomActivity(weeks, days, weekOffset, day);
	}
}
