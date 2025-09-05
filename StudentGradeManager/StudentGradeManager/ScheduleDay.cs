using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static StudentGradeManager.TimetableEntry;

namespace StudentGradeManager
{
    // Represents a single day in the schedule (e.g., Monday, Tuesday)
    public class ScheduleDay
    {
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public List<TimetableEntry> TimetableEntries { get; set; }

        public ScheduleDay(DayOfWeek dayOfWeek, DateTime date)
        {
            DayOfWeek = dayOfWeek;
            Date = date;
            TimetableEntries = new List<TimetableEntry>();
        }
        public void AddTimetableEntry(TimetableEntry newTimetableEntry)
        {
            // TODO
        }
        public void DetailedInfo()
        {
            Console.WriteLine($"Day: {DayOfWeek}");
            foreach (var cls in TimetableEntries)
            {
                Console.WriteLine($"{cls.ToString()}");
            }
        }

        public override string ToString()
        {
            return $"[Day: {DayOfWeek}] Classes: {TimetableEntries.Count}";
        }
    }
}
