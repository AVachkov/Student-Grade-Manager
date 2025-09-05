using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class ReadFromConsole
    {
        private static string SafeReadLine()
        {
            string? input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
        }
        public static string ReadNonEmptyString(string prompt)
        {
            string input;
            Console.Write(prompt);
            while (string.IsNullOrEmpty((input = SafeReadLine())))
            {
                Console.WriteLine("Input cannot be empty. Please try again.");
                Console.Write(prompt);
            }

            return input;
        }
        public static int ReadInteger(string prompt)
        {
            int value;
            Console.Write(prompt);
            while (!int.TryParse(SafeReadLine(), out value))
            {
                Console.WriteLine("Invalid input. Please enter a valid whole number.");
                Console.Write(prompt);
            }

            return value;
        }
        public static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                string input = SafeReadLine().ToLower();

                if (input == "y")
                    return true;
                else if (input == "n")
                    return false;
                else
                    Console.Write("Invalid input. Please enter 'y' or 'n': ");
            }
        }
        public static double ReadDouble(string prompt)
        {
            double value;
            Console.Write(prompt);
            while (!double.TryParse(SafeReadLine(), out value))
            {
                Console.WriteLine("Invalid input. Please enter a valid whole number.");
                Console.Write(prompt);
            }

            return value;
        }
        public static Role ReadUserRole(string prompt)
        {
            Role value;
            Console.Write(prompt);
            while (!Enum.TryParse<Role>(SafeReadLine(), true, out value))
            {
                Console.WriteLine("Invalid input. Please enter a valid role (Student, Teacher, Headmaster).");
                Console.Write(prompt);
            }

            return value;
        }
        public static Subject? ReadSubject(string prompt, Database? db = null, Teacher? teacher = null)
        {
            Console.WriteLine($"Available subjects{(teacher == null ? "" : $" for {teacher.FullName}")}:");
            if (db == null || teacher == null)
            {
                StudentSubjectGrades.ListAllSubjects();
            }
            else
            {
                StudentSubjectGrades.ListAllSubjects(db, teacher);
                teacher.Subjects = db.GetTeacherSubjects(teacher.UserId);
            }

            string input;
            Console.Write(prompt);
            while (true)
            {
                input = SafeReadLine();

                // Handle exit input
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Operation terminated by the user.\n");
                    return null;
                }

                // Check if input matches or contains part of a Subject name
                var matchingSubjects = Enum.GetValues(typeof(Subject))
                                           .Cast<Subject>()
                                           .Where(s => s.ToString().IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                                           .ToList();

                if (matchingSubjects.Count == 1)
                {
                    var selectedSubject = matchingSubjects.First();
                    if (teacher?.Subjects != null && teacher.Subjects.Contains(selectedSubject))
                    {
                        Console.WriteLine($"You selected: {selectedSubject}");
                        return selectedSubject;
                    }
                    else if (teacher != null)
                    {
                        Console.WriteLine($"You are not allowed to grade {selectedSubject}. Type 'exit' to cancel.");
                    }
                    else
                    {
                        Console.WriteLine($"You selected: {selectedSubject}");
                        return selectedSubject;
                    }
                }
                else if (matchingSubjects.Count > 1)
                {
                    Console.WriteLine("Ambiguous input. Did you mean one of the following? Type 'exit' to cancel.");
                    foreach (var subject in matchingSubjects)
                        Console.WriteLine($"- {subject}");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid subject from the list above or type 'exit' to cancel.");
                }

                Console.Write(prompt);
            }
        }
        public static double ProcessGrade(string prompt)
        {
            double value;
            Console.Write(prompt);
            while (!double.TryParse(SafeReadLine(), out value) || value < 2 || value > 6)
            {
                Console.WriteLine("Invalid input. Please enter a valid grade (from 2 to 6).");
                Console.Write(prompt);
            }

            if (value >= 2 && value < 3) return 2;
            if (value >= 3 && value < 3.5) return 3;
            if (value >= 3.5 && value < 4.5) return 4;
            if (value >= 4.5 && value < 5.5) return 5;
            if (value >= 5.5 && value <= 6) return 6;
            return -1; // This line should never be reached
        }
        public static string ReadFeedback(string prompt)
        {
            string input;
            Console.Write(prompt);
            while (true)
            {
                input = SafeReadLine().ToLower();
                if (input == "praise" || input == "remark")
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'praise' or 'remark': ");
                    Console.Write(prompt);
                }
            }
        }
    }
}
