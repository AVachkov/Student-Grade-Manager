using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class StudentSubjectGrades
    {
        public int UserId { get; set; }
        public Subject SubjectTaught { get; set; }
        public List<double> Grades { get; set; }
        public string AddedBy { get; set; }
        public StudentSubjectGrades(int userId, Subject subject, string addedBy)
        {
            UserId = userId;
            SubjectTaught = subject;
            Grades = new List<double>();
            AddedBy = addedBy;
        }
        public static void ListAllSubjects(Database? db = null, User? user = null)
        {
            var allSubjects = Enum.GetValues(typeof(Subject)).Cast<Subject>().ToList();
            if (user is Headmaster || user is Student || db == null || user == null)
            {
                foreach (var subject in allSubjects)
                {
                    Console.WriteLine($"- {subject}");
                }
            }
            else if (user is Teacher teacher)
            {
                var teacherSubjects = db.GetTeacherSubjects(teacher.UserId);
                foreach (var subject in teacherSubjects)
                {
                    if (allSubjects.Contains(subject))
                    {
                        Console.WriteLine($"- {subject}");
                    }
                }
            }
        }
        public override string ToString()
        {
            return $"Subject name: {SubjectTaught.ToString()}, All Grades by {AddedBy}: {(Grades.Any() ? string.Join(", ", Grades) : "No grades")}";
        }
    }
    public enum Subject
    {
        Math,
        BulgarianLanguageAndLiterature,
        English,
        German,
        OtherForeignLanguage,
        ComputerScience,
        History,
        Geography,
        Science,
        Biology,
        Philosophy,
        Physics,
        Art,
        Music,
        PhysicalEducation
    }
}
