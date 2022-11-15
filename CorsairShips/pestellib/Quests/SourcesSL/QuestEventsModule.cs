using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PestelLib.Serialization;
using UnityDI;
using PestelLib.SharedLogicBase;
using S;

namespace PestelLib.SharedLogic.Modules
{
    public class QuestEventsModule : SharedLogicModule<QuestEventsState>
    {
        protected const int QuestsOnBeginningOfEvent = 3;
        [Dependency] protected QuestModule _questModule;
        [Dependency] protected RandomModule _randomModule;
        [GooglePageRef("Quests")] [Dependency] protected List<QuestDef> _questDefs;
       

        [SharedCommand]
        internal void UpdateEvent()
        {
            var currentWeekIndex = GetWeekIndex(CommandTimestamp);
            if (currentWeekIndex != State.WeekIndex)
            {
                //TODO: send notifications and rewards
                _questModule.RemoveQuestByClass(QuestClass.Weekly);
                
                var selectedQuestGroup = currentWeekIndex % (MaxQuestGroupIndex + 1);
                AddNewWeeklyQuests(selectedQuestGroup);
                CheckUnclamedReward(currentWeekIndex);
                State.WeekIndex = currentWeekIndex;
                State.CliamReward = false;
            }
        }

        internal virtual void CheckUnclamedReward(int weekIndex)
        {
        }

        public int SavedWeekIndex
        {
            get { return State.WeekIndex; }
        }

        protected void AddNewWeeklyQuests(int selectedQuestGroup)
        {
            var weeklyQuests = _questDefs
                .Where(x => x.QuestClass == QuestClass.Weekly && x.QuestGroupIndex == selectedQuestGroup).ToList();

            for (var i = 0; i < QuestsOnBeginningOfEvent && weeklyQuests.Count > 0; i++)
            {
                var randomIndex = _randomModule.RandomInt(weeklyQuests.Count);
                var randomQuestDef = weeklyQuests[randomIndex];
                weeklyQuests.RemoveAt(randomIndex);

                _questModule.AddQuest(new QuestState
                {
                    Id = randomQuestDef.Id,
                    Timestamp = CommandTimestamp.Ticks
                });
            }
        }

        protected int MaxQuestGroupIndex
        {
            get
            {
                var weeklyQuests = _questDefs.FindAll(x => x.QuestClass == QuestClass.Weekly);
                var maxWeekIndex = 0;
                foreach (var weeklyQuest in weeklyQuests)
                {
                    if (weeklyQuest.QuestGroupIndex > maxWeekIndex)
                    {
                        maxWeekIndex = weeklyQuest.QuestGroupIndex;
                    }
                }
                return maxWeekIndex;
            }
        }

        public int GetWeekIndex(DateTime now)
        {
            Calendar calendar = DateTimeFormatInfo.InvariantInfo.Calendar;
            return calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public DateTime BeginOfEventTimestamp(DateTime now)
        {
            return FirstDateOfWeekISO8601(now.Year, GetWeekIndex(now));
        }

        public DateTime EndOfEventTimestamp(DateTime now)
        {
            return BeginOfEventTimestamp(now).AddDays(7);
        }

        public TimeSpan Duration(DateTime now)
        {
            return EndOfEventTimestamp(now) - BeginOfEventTimestamp(now);
        }

        public TimeSpan RemainTime(DateTime now)
        {
            return EndOfEventTimestamp(now) - now;
        }

        public TimeSpan TimeFromEventStart(DateTime now)
        {
            return now - BeginOfEventTimestamp(now);
        }

        public float NormalizedDuration(DateTime now)
        {
            return (float)(TimeFromEventStart(now).TotalSeconds / Duration(now).TotalSeconds);
        }
        
        //https://stackoverflow.com/questions/662379/calculate-date-from-week-number
        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
}