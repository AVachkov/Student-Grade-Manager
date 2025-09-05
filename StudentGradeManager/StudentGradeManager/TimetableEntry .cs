using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    // Represents a single entry in the timetable (subject, teacher, room, etc.)
    public class TimetableEntry
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }
        public int TeacherId { get; set; }
        public int RoomId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimetableEntry(int id, Subject subject, int teacherId, int roomId, TimeSpan startTime, TimeSpan endTime)
        {
            Id = id;
            Subject = subject;
            TeacherId = teacherId;
            RoomId = roomId;
            StartTime = startTime;
            EndTime = endTime;
        }

        public override string ToString()
        {
            return $"Subject: {Subject.ToString()} | Teacher ID: {TeacherId} | Room ID: {RoomId} | Time: {StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        }
    }
}
