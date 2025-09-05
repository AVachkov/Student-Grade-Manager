using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public static class ConsoleActions
    {
        public static void CreateAccount(Database db)
        {
            Console.WriteLine("\nCreate Account:");
            string userType = ReadFromConsole.ReadNonEmptyString("Enter user type ('Student' or 'Teacher'): ").ToLower();

            if (userType != "teacher" && userType != "student")
            {
                Console.WriteLine("Invalid user type. Please enter 'Student' or 'Teacher'.");
                return;
            }

            string username = ReadFromConsole.ReadNonEmptyString("Enter Username: ");
            string password = ReadFromConsole.ReadNonEmptyString("Enter Password: ");
            string fullName = ReadFromConsole.ReadNonEmptyString("Enter Full Name: ");

            if (userType.ToLower() == "student")
            {
                string dateInput = ReadFromConsole.ReadNonEmptyString("Enter Date of Birth (dd-MM-yyyy): ");
                DateTime dateOfBirth;
                while (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateOfBirth) || dateOfBirth > DateTime.Now)
                {
                    Console.WriteLine("Invalid date format or future date. Please try again (dd-MM-yyyy).");
                    dateInput = ReadFromConsole.ReadNonEmptyString("Enter Date of Birth (dd-MM-yyyy): ");
                }

                var student = new Student(username, password, fullName, dateOfBirth);

                db.RequestToAddStudent(student);
                Console.WriteLine("Student account creation request has been successfully sent to headmaster.");
            }
            else if (userType.ToLower() == "teacher")
            {
                var teacher = new Teacher(username, password, fullName);

                db.RequestToAddTeacher(teacher);
                Console.WriteLine("Teacher account creation request has been successfully sent to headmaster.");
            }
            else
            {
                Console.WriteLine("Invalid user type. Account creation terminated.");
                return;
            }
        }
        public static void LoginFromConsole(Database db)
        {
            Console.WriteLine("\nLog In:");
            Role userRole = ReadFromConsole.ReadUserRole("Login as 'Headmaster' or 'Teacher' or 'Student': ");

            if (userRole != Role.Headmaster && userRole != Role.Teacher && userRole != Role.Student)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid user role.");
                Console.ResetColor();
                return;
            }

            string username = ReadFromConsole.ReadNonEmptyString("Enter Username: ");
            string password = ReadFromConsole.ReadNonEmptyString("Enter Password: ");

            User? user = db.Login(userRole, username, password);
            if (user != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Login successful.");
                Console.ResetColor();
                PostLoginActions(db, user);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Login failed. Please try again.");
                Console.ResetColor();
            }
        }
        public static void PostLoginActions(Database db, User? user)
        {
            int option = -1;
            switch (user)
            {
                case Headmaster headmaster:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nWelcome, Headmaster {headmaster.Username}!");
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Choose option: ");
                        Console.WriteLine("1. List all students");
                        Console.WriteLine("2. List all teachers");
                        Console.WriteLine("3. List all requests");
                        Console.WriteLine("4. List all pending requests");
                        Console.WriteLine("5. List all announcements");
                        Console.WriteLine("6. Kick out student");
                        Console.WriteLine("7. Kick out teacher");
                        Console.WriteLine("8. Add Subject for Teacher");
                        Console.WriteLine("9. Make Announcements");
                        Console.WriteLine("10. Update User Status");
                        Console.WriteLine("11. Log Out");
                        Console.ResetColor();

                        option = ReadFromConsole.ReadInteger("> ");
                        switch (option)
                        {
                            case 1:
                                headmaster.ListAllStudents(db);
                                break;
                            case 2:
                                headmaster.ListAllTeachers(db);
                                break;
                            case 3:
                                headmaster.ListAllRequests(db);
                                break;
                            case 4:
                                headmaster.ListPendingRequests(db);
                                break;
                            case 5:
                                headmaster.ListAllAnnouncements(db);
                                break;
                            case 6:
                                headmaster.ListAllStudents(db);
                                headmaster.KickOutStudent(db);
                                break;
                            case 7:
                                headmaster.ListAllTeachers(db);
                                headmaster.KickOutTeacher(db);
                                break;
                            case 8:
                                headmaster.ListAllTeachers(db);
                                headmaster.AddSubjectTeacher(db);
                                break;
                            case 9:
                                headmaster.MakeAnnouncement(db);
                                break;
                            case 10:
                                headmaster.ListAllRequests(db);
                                headmaster.ApproveOrRejectUser(db);
                                break;
                            case 11:
                                Console.WriteLine("Successfully logged out.");
                                return;
                            default:
                                Console.WriteLine("Invalid option. Please try again.");
                                break;
                        }
                    }

                case Teacher teacher:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nWelcome, Teacher {teacher.Username}!");
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Choose option: ");
                        Console.WriteLine("1. List Students");
                        Console.WriteLine("2. List Feedbacks");
                        Console.WriteLine("3. View All Students Grades");
                        Console.WriteLine("4. Add Grade");
                        Console.WriteLine("5. Add Feedback");
                        Console.WriteLine("6. Log Out");
                        Console.ResetColor();

                        option = ReadFromConsole.ReadInteger("> ");
                        switch (option)
                        {
                            case 1:
                                teacher.ListAllStudents(db);
                                break;
                            case 2:
                                teacher.ListAllFeedbacks(db);
                                break;
                            case 3:
                                teacher.ListAllStudentsGrades(db); // fix it that it lists only the students that are in the teacher's subject
                                break;
                            case 4:
                                teacher.ListAllStudents(db);
                                teacher.AddGrade(db, teacher);
                                break;
                            case 5:
                                teacher.ListAllStudents(db);
                                teacher.AddFeedback(db);
                                break;
                            case 6:
                                Console.WriteLine("Successfully logged out.");
                                return;
                        }
                    }

                case Student student:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nWelcome, Student {student.Username}!");
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Choose option: ");
                        Console.WriteLine("1. View Grades");
                        Console.WriteLine("2. View Feedbacks");
                        Console.WriteLine("3. Log Out");
                        Console.ResetColor();

                        option = ReadFromConsole.ReadInteger("> ");
                        switch (option)
                        {
                            case 1:
                                student.GetSubjectGrades(db);
                                break;
                            case 2:
                                student.GetFeedbacks(db);
                                break;
                            case 3:
                                Console.WriteLine("Successfully logged out.");
                                return;
                        }

                    }

                default:
                    Console.WriteLine("Unknown user type.");
                    break;
            }
        }
    }
}
