using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomActivity
{
	private readonly OrderedDictionary<int, WeekData> weeks;

	private CurrentDayData currentDay;

	private RoomActivity(OrderedDictionary<int, WeekData> weeks, int week, int day)
	{
		this.weeks = weeks;

		this.Recompute(week, day, (FullWeekData)weeks[week]);
	}

	internal int Score => this.currentDay.Score;

	internal void Update(int week, int day, int amount)
	{
		this.GetWeek(week, day).Increment(day, amount);
	}

	private FullWeekData GetWeek(int week, int day)
	{
		if (this.currentDay.Week == week && this.currentDay.Day == day)
		{
			return this.currentDay.WeekData;
		}

		FullWeekData currentWeekData;
		if (this.currentDay.Week == week)
		{
			currentWeekData = this.currentDay.WeekData;
		}
		else
		{
			this.weeks[week] = currentWeekData = new FullWeekData();

			//Fold old full week data
			for (int i = this.weeks.Count - 1; i >= 0; i--)
			{
				(int pastWeek, WeekData pastWeekData) = this.weeks.GetAt(i);
				if (pastWeek <= week - 2)
				{
					if (pastWeekData is FullWeekData)
					{
						this.weeks.SetAt(i, new ApproximationWeekData(pastWeekData.Total));
					}
					else
					{
						break;
					}
				}
			}
		}

		this.Recompute(week, day, currentWeekData);

		return this.currentDay.WeekData;
	}

	[MemberNotNull(nameof(this.currentDay))]
	private void Recompute(int week, int day, FullWeekData data)
	{
		int score = 0;
		int daysCounted = 0; //Starting at zero which means that the first date we explore has zero modifier.

		//Accurate calculation
		FullWeekData currentWeekData = data;
		int currentWeek = week;
		int currentDay = day - 1; //Skip the first day, that is added on top of the base score.
		for (int j = 0; j < 6; j++)
		{
			if (currentDay < 0)
			{
				if (this.weeks.TryGetValue(week - 1, out WeekData? previousWeekData))
				{
					currentWeek--;
					currentDay = 6;
					currentWeekData = (FullWeekData)previousWeekData;
				}
				else
				{
					break;
				}
			}

			score += this.GetScore(currentWeekData[currentDay--], daysCounted++);
		}

		//We accurately counted six days, consume remaining entries from the week.
		if (currentDay >= 0)
		{
			daysCounted += currentDay;

			int leftOverWeekTotal = 0;
			while (currentDay >= 0)
			{
				leftOverWeekTotal += currentWeekData[currentDay--];
			}

			score += this.GetScore(leftOverWeekTotal, daysCounted);
		}

		//Approximation calculation
		for (int j = this.weeks.IndexOf(currentWeek) - 1; j >= 0; j--)
		{
			score += this.GetScore(this.weeks.GetAt(j).Value.Total, daysCounted += 7);
		}

		this.currentDay = new CurrentDayData(data, week, day, score);
	}

	private int GetScore(int score, int day) => day <= 0
		? score
		: (int)(score * (1 - ((double)day / (1 + day))));

	internal static (int Week, int Day) GetDateParts(DateTimeOffset dateTime)
	{
		const int secondsInADay = 60 * 60 * 24;

		//January 1st of 1970 is Thursday, adjusting it to Monday.
		(long week, long remainder) = Math.DivRem(dateTime.ToUnixTimeSeconds() + (secondsInADay * 3), secondsInADay * 7);

		return ((int)week, (int)(remainder / secondsInADay));
	}

	internal static async Task<RoomActivity> LoadAsync(int week, int day, IAsyncEnumerable<PrivateRoomActivityEntity> values)
	{
		OrderedDictionary<int, WeekData> weeks = [];
		await foreach (PrivateRoomActivityEntity value in values.ConfigureAwait(false))
		{
			if (value.Week == week || value.Week == week - 1)
			{
				weeks[value.Week] = Load(value);
			}
			else
			{
				weeks[value.Week] = new ApproximationWeekData(value.Total);
			}
		}

		return new RoomActivity(weeks, week, day);

		static FullWeekData Load(PrivateRoomActivityEntity value)
		{
			FullWeekData week = new();
			week.Increment(0, value.Monday);
			week.Increment(1, value.Tuesday);
			week.Increment(2, value.Wednesday);
			week.Increment(3, value.Thursday);
			week.Increment(4, value.Friday);
			week.Increment(5, value.Saturday);
			week.Increment(6, value.Sunday);

			return week;
		}
	}

	private sealed class CurrentDayData(FullWeekData weekData, int week, int day, int baseScore)
	{
		internal FullWeekData WeekData { get; } = weekData;

		internal int Week { get; } = week;
		internal int Day { get; } = day;

		private readonly int baseScore = baseScore;

		internal int Score => this.baseScore + this.WeekData[this.Day];
	}

	private abstract class WeekData
	{
		internal abstract int Total { get; }
	}

	private sealed class FullWeekData : WeekData
	{
		private int total;
		private DataHolder data;

		internal override int Total => this.total;

		internal int this[int index] => this.data[index];

		internal void Increment(int index, int value)
		{
			this.data[index] += value;

			this.total += value;
		}

		[InlineArray(7)]
		private struct DataHolder
		{
			private int value;
		}
	}

	private sealed class ApproximationWeekData(int total) : WeekData
	{
		internal override int Total { get; } = total;
	}
}
