using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Combine password and salt, then hash it
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32); // Generate a 256-bit hash

                // Combine the salt and hash for storage
                byte[] hashBytes = new byte[48]; // 16 bytes salt + 32 bytes hash
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                // Convert to Base64 string for storage
                string base64Hash = Convert.ToBase64String(hashBytes);
                return base64Hash;
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (password == storedHash)
            {
                return true;
            }

            try
            {
                // Decode the Base64 encoded hash
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                // Check if the decoded hashBytes length is valid
                if (hashBytes.Length < 48)
                {
                    return password == storedHash;
                }

                // Extract salt and hash
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                byte[] storedPasswordHash = new byte[32];
                Array.Copy(hashBytes, 16, storedPasswordHash, 0, 32);

                // Hash the input password with the extracted salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
                {
                    byte[] testHash = pbkdf2.GetBytes(32);
                    return CryptographicEquals(storedPasswordHash, testHash);
                }
            }
            catch (FormatException)
            {
                // If storedHash is not in Base64 format, assume it's plain text and compare directly
                // Stored hash is not in Base64 format. Falling back to plain-text comparison.
                return password == storedHash;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error during password verification: {e.Message}");
                return false;
            }
        }

        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            // Compare the arrays securely to prevent timing attacks
            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;

        }
    }
}
