namespace SecureDigitalHealthcare.Utilities
{
    using System;
    using System.Security.Cryptography;

    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // Hash the password and salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Combine salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert To base64
            string base64Hash = Convert.ToBase64String(hashBytes);

            // Format hash with extra information
            return string.Format("$HASH$V1${0}${1}", Iterations, base64Hash);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Extract iteration and base64 encoded hash
            var split = hashedPassword.Split('$');
            var iterations = int.Parse(split[3]);
            var base64Hash = split[4];

            // Get salt and hash
            var hashBytes = Convert.FromBase64String(base64Hash);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Compare hashes
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

}
