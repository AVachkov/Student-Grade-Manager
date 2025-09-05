using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;

namespace StudentGradeManager
{
    class Program
    {
        private const string connectionString = 
            "Server=DESKTOP-6PMMSOE\\SQLEXPRESS01;Database=StudentSystem;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;";
        static void Main()
        {
            try
            {
                Database db = new Database(connectionString);

                Console.WriteLine("Welcome to Daskalo!");

                while (true)
                {
                    Console.WriteLine("\nMain menu:");
                    Console.WriteLine("1. Log In");
                    Console.WriteLine("2. Create Account");
                    Console.WriteLine("3. Exit");
                    int choice = ReadFromConsole.ReadInteger("Select an option: ");

                    switch (choice)
                    {
                        case 1:
                            ConsoleActions.LoginFromConsole(db);
                            break;
                        case 2:
                            ConsoleActions.CreateAccount(db);
                            break;
                        case 3:
                            Console.WriteLine("\nGoodbye!");
                            return;
                        default:
                            Console.WriteLine("\nInvalid option. Please select a valid menu option.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name} error: {ex.Message}");
            }
        }
    }
}