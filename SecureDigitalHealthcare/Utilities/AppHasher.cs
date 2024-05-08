using Microsoft.AspNetCore.Identity;

namespace SecureDigitalHealthcare.Utilities
{
    public class AppHasher
    {
        public static string HashPassword(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);

            return hashedPassword;
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            bool verified = BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);

            return verified;
        }
    }
}
