using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class Student : User
    {
        public DateTime DateOfBirth { get; set; }
        public List<StudentSubjectGrades> SubjectGrades { get; set; }
        public string Status { get; set; }

        public Student(string username, string password, string fullName, DateTime dateOfBirth)
            : base(username, password, fullName)
        {
            DateOfBirth = dateOfBirth;
            SubjectGrades = new List<StudentSubjectGrades>();
        }
        public void GetSubjectGrades(Database db)
        {
            var subjectGrades = db.GetStudentSubjectGrades(this.UserId);
            if (subjectGrades.Count == 0)
            {
                Console.WriteLine("\nNo subjects taught yet.");
                return;
            }

            foreach (var subjectGrade in subjectGrades)
            {
                Console.WriteLine(subjectGrade);
            }
        }
        public void GetFeedbacks(Database db)
        {
            var feedbacks = db.GetAllStudentFeedbacks(this.UserId);

            if (feedbacks.Count == 0)
            {
                Console.WriteLine("\nNo feedbacks found.");
                return;
            }

            foreach (var feedback in feedbacks)
            {
                feedback.DetailedInfo(db);
            }
        }
        public void PrintDetailedData()
        {
            Console.WriteLine("Student Details:");
            Console.WriteLine($"- Full Name: {FullName}");
            Console.WriteLine($"- Username: {Username}");
            Console.WriteLine($"- Date of Birth: {DateOfBirth:dd-MM-yyyy}");
            Console.WriteLine($"- User Role: {UserRole}");
            Console.WriteLine($"- Subject Grades:");
            if (SubjectGrades.Any())
            {
                foreach (var subjectGrade in SubjectGrades)
                {
                    Console.WriteLine($"  - {subjectGrade.SubjectTaught}: {string.Join(", ", subjectGrade.Grades)}");
                }
            }
            else
            {
                Console.WriteLine("  No subjects taught yet.");
            }
        }
        public override void ContactHeadmaster(Database db)
        {
            base.ContactHeadmaster(db);
        }
        public override string ToString()
        {
            return $"Student [ID: {UserId}]: {FullName}, Username: {Username}, Date of Birth: {DateOfBirth:dd-MM-yyyy}";
        }
    }
}
