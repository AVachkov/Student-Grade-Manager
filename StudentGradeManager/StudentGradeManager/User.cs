using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public abstract class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public Role UserRole { get; set; }

        protected User(string username, string password, string fullName)
        {
            Username = username;
            PasswordHash = password;
            FullName = fullName;

            if (this is Headmaster)
            {
                UserRole = Role.Headmaster;
            }
            else if (this is Teacher)
            {
                UserRole = Role.Teacher;
            }
            else if (this is Student)
            {
                UserRole = Role.Student;
            }
        }
        public virtual void ContactHeadmaster(Database db)
        {
            int counter = 1;
            var allContactInfo = db.GetAllHeadmastersContactInfo();
            foreach (var contactInfo in allContactInfo)
            {
                Console.WriteLine($"Headmaster {counter++} contact info: {contactInfo}");
            }
        }
        public override string ToString()
        {
            return $"{GetType().Name.ToString()}. Username: {Username.Trim()}, Full name: {FullName.Trim()}, Role: {UserRole.ToString()}";
        }
    }
    public enum Role
    {
        Headmaster,
        Teacher,
        Student
    }
}
