using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class Teacher : User
    {
        public List<Subject> Subjects { get; set; }
        public string Status { get; set; }
        public Teacher(string username, string password, string fullName)
            : base(username, password, fullName)
        {
            Subjects = new List<Subject>();
        }
        public void AddGrade(Database db, Teacher teacher)
        {
            int userId = ReadFromConsole.ReadInteger("Enter student ID: ");
            if (!db.DoesStudentExist(userId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No student with ID {userId} found.\n");
                Console.ResetColor();
                return;
            }
            Subject? subject = ReadFromConsole.ReadSubject("Enter subject: ", db, teacher);
            if (subject == null)
                return;

            double grade = ReadFromConsole.ProcessGrade("Enter grade: ");

            db.AddStudentGrade(userId, subject, grade, teacher.FullName);
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
        public void ListAllStudentsGrades(Database db)
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
                    List<StudentSubjectGrades> grades = db.GetStudentSubjectGrades(student.UserId);
                    if (grades.Count == 0)
                    {
                        Console.WriteLine($"\nNo grades for {student.FullName}");
                        continue;
                    }
                    else
                    {
                        foreach (StudentSubjectGrades grade in grades)
                        {
                            Console.WriteLine(grade);
                        }
                    }
                }
            }
        }
        public void ListAllFeedbacks(Database db)
        {
            var feedbacks = db.GetAllTeacherFeedbacks(this.FullName);

            if (feedbacks.Count == 0)
            {
                Console.WriteLine($"\nNo feedbacks by {this.FullName}");
                return;
            }
            else
            {
                foreach (var feedback in feedbacks)
                {
                    feedback.DetailedInfo(db);
                }
            }
        }
        public void GetSubjects(Database db, int teacherId)
        {
            var subjects = db.GetTeacherSubjects(teacherId);
            if (subjects.Count == 0)
            {
                Console.WriteLine("\nNo subjects taught yet.");
                return;
            }
            foreach (var subject in subjects)
            {
                Console.WriteLine(subject);
            }
        }
        public override void ContactHeadmaster(Database db)
        {
            base.ContactHeadmaster(db);
        }
        public void AddFeedback(Database db)
        {
            int studentId = ReadFromConsole.ReadInteger("Enter student ID: ");
            if (!db.DoesStudentExist(studentId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No student with ID {studentId} found.\n");
                Console.ResetColor();
                return;
            }

            string praiseOrRemark = ReadFromConsole.ReadFeedback("Enter 'praise' or 'remark': ");
            string content = ReadFromConsole.ReadNonEmptyString("Enter feedback info: ");

            praiseOrRemark = char.ToUpper(praiseOrRemark[0]) + praiseOrRemark.Substring(1);

            db.TeacherAddFeedback(studentId, praiseOrRemark, content);
        }
        public override string ToString()
        {
            return $"Teacher [ID: {UserId}]: {FullName}, Username: {Username}, Subjects: {(Subjects.Any() ? string.Join(", ", Subjects) : "None")}";
        }
        public void PrintDetailedData()
        {
            Console.WriteLine("Teacher Details:");
            Console.WriteLine($"- Full Name: {FullName}");
            Console.WriteLine($"- Username: {Username}");
            Console.WriteLine($"- User Role: {UserRole}");
            Console.WriteLine($"- All Subjects: {(Subjects.Any() ? string.Join(", ", Subjects) : "No subjects assigned.")}");
        }
    }
}
