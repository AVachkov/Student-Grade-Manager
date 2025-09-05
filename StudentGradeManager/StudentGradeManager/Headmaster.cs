using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudentGradeManager
{
    public class Headmaster : User
    {
        public string SchoolName { get; set; }
        public List<string> Announcements { get; set; }
        public string ContactInfo { get; set; }
        public Headmaster(string username, string password, string fullName, string schoolName, string contactInfo)
            : base(username, password, fullName)
        {
            SchoolName = schoolName;
            Announcements = new List<string>();
            ContactInfo = contactInfo;
        }
        public void ApproveOrRejectUser(Database db)
        {
            int requestId = ReadFromConsole.ReadInteger("Enter request ID: ");
            bool isApproved = ReadFromConsole.ReadYesNo("Approve registration?");
            string status = isApproved ? "Approved" : "Rejected";
            db.UpdateRequestStatus(requestId, status);
        }
        public void ListAllStudents(Database db)
        {
            var students = db.GetStudents();

            if (students.Count == 0)
            {
                Console.WriteLine("\nNo students found.");
                return;
            }
            else
            {
                foreach (var student in students)
                {
                    Console.WriteLine(student);
                }
            }
        }
        public void ListAllTeachers(Database db)
        {
            var teachers = db.GetTeachers();

            if (teachers.Count == 0)
            {
                Console.WriteLine("\nNo teachers found.");
                return;
            }
            else
            {
                foreach (var teacher in teachers)
                {
                    Console.WriteLine(teacher);
                }
            }
        }
        public void ListAllRequests(Database db)
        {
            var requests = db.GetAllRequests();
            if (requests.Count == 0)
            {
                Console.WriteLine("\nNo requests found.");
            }
            else
            {
                foreach (User request in requests)
                {
                    if (request is Student student)
                    {
                        Console.WriteLine($"Request ID: {student.UserId}, {student}, status - {student.Status}");
                    }
                    else if (request is Teacher teacher)
                    {
                        Console.WriteLine($"Request ID: {teacher.UserId}, {teacher}, status - {teacher.Status}");
                    }
                }
            }
        }
        public void ListPendingRequests(Database db)
        {
            var requests = db.GetAllRequests();
            if (requests.Count == 0)
            {
                Console.WriteLine("\nNo requests found.");
                return;
            }

            var pending = requests.Where(u =>
            (u is Student s && s.Status == "Pending") ||
            (u is Teacher t && t.Status == "Pending"))
                .ToList();
            if (pending.Count == 0)
            {
                Console.WriteLine("\nNo pending requests found.");
                return;
            }
            foreach (User request in pending)
            {
                if (request is Student student)
                {
                    Console.WriteLine($"{student.Status}, Request ID: {student.UserId}, {student}");
                }
                else if (request is Teacher teacher)
                {
                    Console.WriteLine($"{teacher.Status}, Request ID: {teacher.UserId}, {teacher}");
                }
            }
        }
        public void ListAllAnnouncements(Database db)
        {
            var announcements = db.GetAnnouncements();
            if (announcements.Count == 0)
            {
                Console.WriteLine("\nNo announcements found.");
            }
            else
            {
                foreach (var announcement in announcements)
                {
                    Console.WriteLine(announcement);
                }
            }
        }
        public void MakeAnnouncement(Database db)
        {
            string announcementInfo = ReadFromConsole.ReadNonEmptyString("Enter announcement info: ");
            int headmasterId = ReadFromConsole.ReadInteger("Enter headmaster ID: ");
            db.MakeAnnouncements(db, announcementInfo, headmasterId);

            Console.WriteLine("Announcement created successfully.");
        }
        public void KickOutTeacher(Database db)
        {
            var teachers = db.GetTeachers();
            if (teachers.Count == 0) return;

            int teacherId = ReadFromConsole.ReadInteger("Enter Teacher ID: ");
            db.DeleteTeacher(teacherId);
            Console.WriteLine("Teacher kicked out successfully.");
        }
        public void KickOutStudent(Database db)
        {
            var students = db.GetStudents();
            if (students.Count == 0) return;

            int studentId = ReadFromConsole.ReadInteger("Enter Student ID: ");
            db.DeleteStudent(studentId);
        }
        public void AddSubjectTeacher(Database db)
        {
            int teacherId = ReadFromConsole.ReadInteger("Enter Teacher ID: ");
            Subject? subject = ReadFromConsole.ReadSubject("Enter subject: ");
            db.AddTeacherSubject(subject, teacherId);
        }
        public override string ToString()
        {
            return $"Headmaster: {FullName}, School: {SchoolName}, Contact: {ContactInfo}";
        }
        public void PrintDetailedData()
        {
            Console.WriteLine($"Headmaster Details:");
            Console.WriteLine($"- Full Name: {FullName}");
            Console.WriteLine($"- School Name: {SchoolName}");
            Console.WriteLine($"- Contact Info: {ContactInfo}");
            Console.WriteLine($"- Announcements: {(Announcements.Any() ? string.Join("; ", Announcements) : "No announcements available.")}");
        }
    }
}
