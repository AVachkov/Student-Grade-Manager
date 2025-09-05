using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    // Represents the full weekly schedule containing all days
    public class WeeklySchedule
    {
        public List<ScheduleDay> Days { get; set; } = new List<ScheduleDay>();
        public WeeklySchedule()
        {
            // Initialize the schedule with all days of the week
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                Days.Add(new ScheduleDay(day));
            }
        }

        public void AddDay(ScheduleDay day)
        {
            // TODO
        }

        public void DisplayTimetable()
        {
            foreach (ScheduleDay day in Days)
            {
                Console.WriteLine(day.ToString());
            }
        }
        public override string ToString()
        {
            return $"[Weekly Schedule] Days: {Days.Count}";
        }
    }
}
