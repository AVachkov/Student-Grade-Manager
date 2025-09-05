using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class Database
    {
        private readonly string _connectionString;
        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Create a new SqlConnection and open it
        private SqlConnection GetSqlConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        // Console <-> DB account actions:
        public void RequestToAddStudent(Student student)
        {
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = "INSERT INTO AccountRequests(Username, Password, FullName, DateOfBirth, UserType, Status) OUTPUT INSERTED.UserId " +
                            "VALUES (@Username, @Password, @FullName, @DateOfBirth, @UserType, @Status)";
                        int studentId;
                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Username", student.Username);
                            command.Parameters.AddWithValue("@Password", PasswordHasher.HashPassword(student.PasswordHash));
                            command.Parameters.AddWithValue("@FullName", student.FullName);
                            command.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
                            command.Parameters.AddWithValue("@UserType", "Student");
                            command.Parameters.AddWithValue("@Status", "Pending");
                            var result = command.ExecuteScalar();
                            if (result == DBNull.Value || result == null)
                            {
                                throw new InvalidOperationException("Failed to retrieve the new StudentId.");
                            }
                            studentId = Convert.ToInt32(result);
                            student.UserId = studentId;
                        }
                        // no grades at first

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public void RequestToAddTeacher(Teacher teacher)
        {
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = "INSERT INTO AccountRequests(Username, Password, FullName, DateOfBirth, UserType, Status) OUTPUT INSERTED.RequestId " +
                            "VALUES (@Username, @Password, @FullName, @DateOfBirth, @UserType, @Status)";
                        int teacherId;
                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Username", teacher.Username);
                            command.Parameters.AddWithValue("@Password", PasswordHasher.HashPassword(teacher.PasswordHash));
                            command.Parameters.AddWithValue("@FullName", teacher.FullName);
                            command.Parameters.AddWithValue("@DateOfBirth", DBNull.Value); // No date of birth for teachers
                            command.Parameters.AddWithValue("@UserType", "Teacher");
                            command.Parameters.AddWithValue("@Status", "Pending");
                            var result = command.ExecuteScalar();
                            if (result == DBNull.Value || result == null)
                            {
                                throw new InvalidOperationException("Failed to retrieve the new TeacherId.");
                            }
                            teacherId = Convert.ToInt32(result);
                            teacher.UserId = teacherId;
                        }
                        // no subjects at first

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public User? Login(Role userRole, string username, string password)
        {
            string? query = userRole switch
            {
                Role.Headmaster => "SELECT Id, Username, PasswordHash, FullName, SchoolName, ContactInfo FROM Headmasters WHERE Username = @Username",
                Role.Student => "SELECT StudentId, Username, PasswordHash, FullName, DateOfBirth FROM Students WHERE Username = @Username",
                Role.Teacher => "SELECT TeacherId, Username, PasswordHash, FullName FROM Teachers WHERE Username = @Username",
                _ => null
            };

            if (query == null)
            {
                Console.WriteLine("Unsupported role.");
                return null;
            }

            using (var connection = GetSqlConnection())
            {
                // check if the account is being pending approval to output appropriate message
                if (userRole != Role.Headmaster)
                {
                    if (!CheckAccountStatus(connection, username))
                    {
                        return null; // Account is either pending or rejected
                    }
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("User not found.");
                            return null;
                        }

                        string storedHash = reader["PasswordHash"]?.ToString() ?? throw new InvalidOperationException("PasswordHash is missing.");
                        if (!PasswordHasher.VerifyPassword(password, storedHash))
                        {
                            Console.WriteLine("Invalid password");
                            return null;
                        }

                        return CreateUserFromReader(userRole, reader);
                    }
                }
            }
        }
        public Student? GetStudentByID(int studentId)
        {
            if (studentId <= 0)
            {
                Console.WriteLine("Invalid student ID provided.");
                return null;
            }

            string query = "SELECT Username, PasswordHash, FullName, DateOfBirth FROM Students WHERE StudentId = @StudentId";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"]?.ToString() ?? throw new InvalidOperationException("Username is missing.");
                            string password = reader["PasswordHash"]?.ToString() ?? throw new InvalidOperationException("PasswordHash is missing.");
                            string fullName = reader["FullName"]?.ToString() ?? throw new InvalidOperationException("FullName is missing.");
                            DateTime birth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth"));

                            Student student = new Student(username, password, fullName, birth)
                            {
                                UserId = studentId
                            };

                            return student;
                        }
                        else
                        {
                            Console.WriteLine($"No student found with ID {studentId}.");
                            return null;
                        }
                    }
                }
            }
        }
        public List<string> GetAllHeadmastersContactInfo()
        {
            string query = "SELECT ContactInfo FROM Headmasters";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var contactInfo = new List<string>();
                        while (reader.Read())
                        {
                            string? contact = reader["ContactInfo"].ToString();
                            if (!string.IsNullOrEmpty(contact))
                            {
                                contactInfo.Add(contact);
                            }
                        }
                        return contactInfo;
                    }
                }
            }
        }
        public bool DoesStudentExist(int studentId)
        {
            if (studentId <= 0)
            {
                Console.WriteLine("Invalid student ID provided.");
                return false;
            }

            string query = "SELECT COUNT(*) FROM Students WHERE StudentId = @StudentId";
            using (var connection = GetSqlConnection())
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
        // helper methods
        private bool CheckAccountStatus(SqlConnection connection, string username)
        {
            string approvalQuery = "SELECT Status FROM AccountRequests WHERE Username = @Username";
            using (var command = new SqlCommand(approvalQuery, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                var status = command.ExecuteScalar()?.ToString()?.ToLower();

                if (status == "pending")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nYour account is pending approval. Please contact administrator for more info.");
                    Console.ResetColor();
                    return false;
                }
                else if (status == "rejected")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nYour account has been rejected.");
                    Console.ResetColor();
                    return false;
                }
            }
            return true; // Account is approved
        }
        private User CreateUserFromReader(Role userRole, SqlDataReader reader)
        {
            return userRole switch
            {
                Role.Headmaster => new Headmaster(
                    reader["Username"].ToString(),
                    reader["PasswordHash"].ToString(),
                    reader["FullName"].ToString(),
                    reader["SchoolName"].ToString(),
                    reader["ContactInfo"].ToString())
                {
                    UserId = Convert.ToInt32(reader["Id"]),
                    UserRole = Role.Headmaster
                },
                Role.Student => new Student(
                    reader["Username"].ToString(),
                    reader["PasswordHash"].ToString(),
                    reader["FullName"].ToString(),
                    Convert.ToDateTime(reader["DateOfBirth"]))
                {
                    UserId = Convert.ToInt32(reader["StudentId"]),
                    UserRole = Role.Student
                },
                Role.Teacher => new Teacher(
                    reader["Username"].ToString(),
                    reader["PasswordHash"].ToString(),
                    reader["FullName"].ToString())
                {
                    UserId = Convert.ToInt32(reader["TeacherId"]),
                    UserRole = Role.Teacher
                },
                _ => throw new InvalidOperationException("Unsupported role.")
            };
        }

        // Headmaster actions:
        public void AddHeadmaster(Headmaster headmaster)
        {
            try
            {
                string query = "INSERT INTO Headmasters(Username, PasswordHash, FullName, SchoolName, ContactInfo) " +
                    "VALUES (@Username, @PasswordHash, @FullName, @SchoolName, @ContactInfo)";

                using (var connection = GetSqlConnection())
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", headmaster.Username);
                        command.Parameters.AddWithValue("@PasswordHash", PasswordHasher.HashPassword(headmaster.PasswordHash));
                        command.Parameters.AddWithValue("@FullName", headmaster.FullName);
                        command.Parameters.AddWithValue("@SchoolName", headmaster.SchoolName);
                        command.Parameters.AddWithValue("@ContactInfo", headmaster.ContactInfo);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void MakeAnnouncements(Database db, string announcementInfo, int headmasterId)
        {
            string query = "INSERT INTO Announcements(HeadmasterId, AnnouncementInfo) VALUES (@HeadmasterId, @AnnouncementInfo)";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeadmasterId", headmasterId);
                    command.Parameters.AddWithValue("@AnnouncementInfo", announcementInfo);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<string> GetAnnouncements()
        {
            var announcements = new List<string>();
            string query = "SELECT AnnouncementInfo FROM Announcements";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string announcementInfo = reader.GetString(0);
                            announcements.Add(announcementInfo);
                        }
                    }
                }
            }

            return announcements;
        }
        public List<Teacher> GetTeachers()
        {
            var teachers = new List<Teacher>();

            string query = "SELECT TeacherId, Username, PasswordHash, FullName FROM Teachers";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var teacher = new Teacher(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                            {
                                UserId = reader.GetInt32(0)
                            };

                            teacher.Subjects = GetTeacherSubjects(teacher.UserId);
                            teachers.Add(teacher);
                        }
                    }
                }
            }

            return teachers;
        }
        public List<User> GetAllRequests()
        {
            List<User> requests = new List<User>();
            string query = "SELECT * FROM AccountRequests";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userType = reader.GetString(5);
                            if (userType.ToLower() == "student")
                            {
                                var student = new Student(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDateTime(4))
                                {
                                    UserId = reader.GetInt32(0),
                                    Status = reader.GetString(6)
                                };
                                requests.Add(student);
                            }
                            else if (userType.ToLower() == "teacher")
                            {
                                var teacher = new Teacher(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                                {
                                    UserId = reader.GetInt32(0),
                                    Status = reader.GetString(6)
                                };
                                requests.Add(teacher);
                            }
                        }
                    }
                }
            }

            return requests;
        }
        public void UpdateRequestStatus(int requestId, string status) // if status is "Approved" - append the user to the database and update AccountRequests Status, if "Rejected" - delete the user from AccountRequests
        {
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if the request exists and if the status is valid
                        string selectActionTypeQuery = "SELECT Status FROM AccountRequests WHERE RequestId = @RequestId";
                        using (var selectCommand = new SqlCommand(selectActionTypeQuery, connection, transaction))
                        {
                            selectCommand.Parameters.AddWithValue("@RequestId", requestId);
                            var result = selectCommand.ExecuteScalar();
                            if (result == null)
                            {
                                Console.WriteLine($"Request with ID {requestId} not found.");
                                return;
                            }
                            else if (result.ToString() == status)
                            {
                                Console.WriteLine($"Request with ID {requestId} is already in the '{status}' state.");
                                return;
                            }
                            else if (result.ToString() == "Approved" && status == "Rejected")
                            {
                                Console.WriteLine($"Cannot change request with ID {requestId} from 'Approved' to 'Rejected'.");
                                return;
                            }
                            else if (result.ToString() == "Rejected" && status == "Approved")
                            {
                                Console.WriteLine($"Cannot change request with ID {requestId} from 'Rejected' to 'Approved'.");
                                return;
                            }
                        }

                        // Update the status of the request
                        string updateQuery = "UPDATE AccountRequests SET Status = @Status WHERE RequestId = @RequestId";
                        using (var command = new SqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Status", status);
                            command.Parameters.AddWithValue("@RequestId", requestId);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                Console.WriteLine($"No rows updated for RequestId: {requestId}");
                                return;
                            }
                        }

                        // If the request is approved, insert the user into the appropriate table
                        string query = "SELECT * FROM AccountRequests WHERE RequestId = @RequestId";
                        User? user = null;

                        if (status.ToLower() == "approved")
                        {
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@RequestId", requestId);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string username = reader.GetString(1);
                                        string password = reader.GetString(2);
                                        string fullName = reader.GetString(3);
                                        string userType = reader.GetString(5).ToLower();
                                        if (userType == "student")
                                        {
                                            DateTime dateOfBirth = reader.GetDateTime(4);

                                            user = new Student(username, password, fullName, dateOfBirth);
                                        }
                                        else if (userType == "teacher")
                                        {
                                            user = new Teacher(username, password, fullName);
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Unknown user type: {userType}");
                                            return;
                                        }
                                    }
                                }
                            }

                            // Insert the user into the appropriate table
                            string insertQuery = user?.GetType().Name.ToLower() switch
                            {
                                "student" => "INSERT INTO Students(Username, PasswordHash, FullName, DateOfBirth) OUTPUT INSERTED.StudentId VALUES (@Username, @PasswordHash, @FullName, @DateOfBirth)",
                                "teacher" => "INSERT INTO Teachers(Username, PasswordHash, FullName) OUTPUT INSERTED.TeacherId VALUES (@Username, @PasswordHash, @FullName)",
                                _ => throw new Exception("Not supported user type.")
                            };

                            using (var command = new SqlCommand(insertQuery, connection, transaction))
                            {

                                Student student = null;
                                Teacher teacher = null;
                                if (user?.GetType().Name.ToLower() == "student")
                                {
                                    student = (Student)user;
                                    command.Parameters.AddWithValue("@Username", student.Username);
                                    command.Parameters.AddWithValue("@PasswordHash", student.PasswordHash); // already hashed in RequestToAddStudent() || RequestToAddTeacher()
                                    command.Parameters.AddWithValue("@FullName", student.FullName);

                                    command.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
                                }
                                else if (user?.GetType().Name.ToLower() == "teacher")
                                {
                                    teacher = (Teacher)user;
                                    command.Parameters.AddWithValue("@Username", teacher.Username);
                                    command.Parameters.AddWithValue("@PasswordHash", teacher.PasswordHash); // already hashed in RequestToAddStudent() || RequestToAddTeacher()
                                    command.Parameters.AddWithValue("@FullName", teacher.FullName);
                                }

                                var result = command.ExecuteScalar();
                                if (result == null)
                                {
                                    Console.WriteLine($"Failed to insert {user?.GetType().Name} into the database.");
                                    return;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\nUser {(string.IsNullOrEmpty(student?.Username) ? teacher?.Username ?? "Unknown" : student.Username ?? "Unknown")} has been successfully approved!");
                                    Console.ResetColor();
                                }
                            }
                        }
                        else if (status.ToLower() == "rejected")
                        {
                            string deleteQuery = "DELETE FROM AccountRequests WHERE RequestId = @RequestId";
                            using (var command = new SqlCommand(deleteQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@RequestId", requestId);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    Console.WriteLine($"No rows deleted for RequestId: {requestId}");
                                    return;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"\nUser {user?.Username} has been rejected!");
                                    Console.ResetColor();
                                }
                            }
                        }

                        // Verify the update within the same transaction
                        string verifyQuery = "SELECT Status FROM AccountRequests WHERE RequestId = @RequestId";
                        using (var verifyCommand = new SqlCommand(verifyQuery, connection, transaction))
                        {
                            verifyCommand.Parameters.AddWithValue("@RequestId", requestId);
                            var result = verifyCommand.ExecuteScalar();
                            Console.WriteLine($"Status after update: {result}");
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public void DeleteStudent(int studentId)
        {
            string gradesQuery = "DELETE FROM StudentSubjectGrades WHERE StudentId = @StudentId";
            string studentQuery = "DELETE FROM Students WHERE StudentId = @StudentId";
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // delete subjects and grades first
                        using (var command = new SqlCommand(gradesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StudentId", studentId);
                            command.ExecuteNonQuery();
                        }

                        // then student
                        using (var command = new SqlCommand(studentQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StudentId", studentId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }
        public void DeleteTeacher(int teacherId)
        {
            string subjectsQuery = "DELETE FROM TeacherSubjects WHERE TeacherId = @TeacherId";
            string teacherQuery = "DELETE FROM Teachers WHERE TeacherId = @TeacherId";
            using (var connection = GetSqlConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(subjectsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TeacherId", teacherId);
                            command.ExecuteNonQuery();
                        }

                        using (var command = new SqlCommand(teacherQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TeacherId", teacherId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }
        public void AddTeacherSubject(Subject? subject, int teacherId)
        {
            string query = "INSERT INTO TeacherSubjects (TeacherId, Subject) SELECT @TeacherId, @Subject FROM Teachers WHERE Teachers.TeacherId = @TeacherId";
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TeacherId", teacherId);
                            command.Parameters.AddWithValue("@Subject", (int)subject);
                            var result = command.ExecuteNonQuery();

                            if (result == 0)
                            {
                                Console.WriteLine($"Failed to add subject {subject} for teacher with ID {teacherId}.");
                            }
                            else
                            {
                                Console.WriteLine($"Subject {subject} added successfully for teacher with ID {teacherId}.");
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public List<Student> GetStudents()
        {
            var students = new List<Student>();
            string query = "SELECT StudentId, Username, PasswordHash, FullName, DateOfBirth FROM Students";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var student = new Student(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDateTime(4))
                            {
                                UserId = reader.GetInt32(0)
                            };

                            student.SubjectGrades = GetStudentSubjectGrades(student.UserId);
                            students.Add(student);
                        }
                    }
                }
            }

            return students;
        }

        // Teacher and Headmaster actions:
        public List<Subject> GetTeacherSubjects(int teacherId)
        {
            var subjects = new List<Subject>();
            string query = "SELECT Subject FROM TeacherSubjects WHERE TeacherId = @TeacherId";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subject subject = (Subject)reader.GetInt32(0);

                            subjects.Add(subject);
                        }
                    }
                }
            }

            return subjects;
        }
        public void DeleteTeacherSubject(Subject subject, int teacherId)
        {
            string query = "DELETE FROM TeacherSubjects WHERE TeacherId = @TeacherId AND Subject = @Subject";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                    command.Parameters.AddWithValue("@Subject", (int)subject);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddStudentGrade(int studentId, Subject? subject, double grade, string teacherFullNameOrUsername)
        {
            string query = "INSERT INTO StudentSubjectGrades(StudentId, Subject, Grade, Added_By) VALUES (@StudentId, @Subject, @Grade, @Added_By)";
            using (var connection = GetSqlConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StudentId", studentId);
                            command.Parameters.AddWithValue("@Subject", (int)subject);
                            command.Parameters.AddWithValue("@Grade", grade);
                            command.Parameters.AddWithValue("@Added_By", teacherFullNameOrUsername);
                            var result = command.ExecuteNonQuery();
                            if (result == 0)
                            {
                                Console.WriteLine($"Failed to add grade {grade} for student with ID {studentId}.");
                            }
                            else
                            {
                                var student = GetStudentByID(studentId);
                                Console.WriteLine($"Grade {grade} added successfully for student {student.Username} ({student.FullName}).");
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Headmaster, Teacher and Student actions:
        public List<StudentSubjectGrades> GetStudentSubjectGrades(int studentId)
        {
            var subjectGrades = new List<StudentSubjectGrades>();
            string query = "SELECT Subject, Grade, Added_By FROM StudentSubjectGrades WHERE StudentId = @StudentId";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subject subject = (Subject)reader.GetInt32(0);
                            double grade = reader.GetDouble(1);
                            string addedBy = reader.GetString(2);

                            StudentSubjectGrades? subjectGrade = subjectGrades.FirstOrDefault(sg => sg.SubjectTaught == subject);
                            if (subjectGrade == null)
                            {
                                subjectGrade = new StudentSubjectGrades(studentId, subject, addedBy);
                                subjectGrades.Add(subjectGrade);
                            }
                            subjectGrade.Grades.Add(grade);
                        }
                    }
                }
            }

            return subjectGrades;
        }

        // Teacher actions:
        public void TeacherAddGrade(int studentId, Subject subject, double grade)
        {
            string query = "INSERT INTO StudentSubjectGrades(StudentId, Subject, Grade) VALUES (@StudentId, @Subject, @Grade)";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@Subject", (int)subject);
                    command.Parameters.AddWithValue("@Grade", grade);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void TeacherAddFeedback(int studentId, string praiseOrRemark, string info)
        {
            string query = "INSERT INTO Feedbacks(StudentId, PraiseOrRemark, Info) VALUES (@StudentId, @PraiseOrRemark, @Info)";
            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@PraiseOrRemark", praiseOrRemark);
                    command.Parameters.AddWithValue("@Info", info);
                    var result = command.ExecuteNonQuery();

                    if (result == 0)
                    {
                        Console.WriteLine($"Failed to add feedback for student with ID {studentId}.");
                    }
                    else
                    {
                        Console.WriteLine($"Feedback added successfully for student with ID {studentId}.");
                    }
                }
            }
        }
        public List<Feedback> GetAllTeacherFeedbacks(string teacherFullName)
        {
            var feedbacks = new List<Feedback>();
            string query = "SELECT StudentId, PraiseOrRemark, Info, Added_By FROM Feedbacks WHERE Added_By = @Added_By";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Added_By", teacherFullName);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var praiseOrRemark = reader.GetString(1);
                            var info = reader.GetString(2);
                            var addedBy = reader.GetString(3);
                            feedbacks.Add(new Feedback(praiseOrRemark, info, addedBy)
                            {
                                UserId = reader.GetInt32(0)
                            });
                        }
                    }
                }
            }

            return feedbacks;
        }

        // Student actions:
        public List<Feedback> GetAllStudentFeedbacks(int studentId)
        {
            var feedbacks = new List<Feedback>();
            string query = "SELECT StudentId, PraiseOrRemark, Info, Added_By FROM Feedbacks WHERE StudentId = @StudentId";

            using (var connection = GetSqlConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var praiseOrRemark = reader.GetString(1);
                            var info = reader.GetString(2);
                            var addedBy = reader.GetString(3);
                            feedbacks.Add(new Feedback(praiseOrRemark, info, addedBy)
                            {
                                UserId = studentId
                            });
                        }
                    }
                }
            }

            return feedbacks;
        }
    }
}
