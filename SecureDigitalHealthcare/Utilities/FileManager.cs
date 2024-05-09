using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureDigitalHealthcare.Utilities
{
    public class FileManager
    {

        public static bool SaveProfileImage(IWebHostEnvironment _environment, IFormFile profilePictureInput, out string fileName)
        {
            fileName = string.Empty;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(profilePictureInput.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return false;
            }

            // Generate a unique filename
            fileName = Guid.NewGuid().ToString() + fileExtension;
            // Store files outside public folders
            var folder = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName);
            var filePath = Path.Combine(folder, fileName);


            while (File.Exists(filePath))
            {
                // If the file already exists, generate a new filename
                fileName = Guid.NewGuid().ToString() + fileExtension;
                filePath = Path.Combine(folder, fileName);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                profilePictureInput.CopyTo(fileStream);
            }

            return true;
        }
        public static bool DeleteProfileImage(IWebHostEnvironment _environment, string fileName)
        {
            // Store files outside public folders
            var folder = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName);
            var filePath = Path.Combine(folder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }


            return false;
        }
        public static void EncryptFile(string filePath, byte[] key)
        {
            string tempFileName = Path.GetTempFileName();

            using (SymmetricAlgorithm cipher = Aes.Create())
            using (FileStream fileStream = File.OpenRead(filePath))
            using (FileStream tempFile = File.Create(tempFileName))
            {
                cipher.Key = key;
                // aes.IV will be automatically populated with a secure random value
                byte[] iv = cipher.IV;

                // Write a marker header so we can identify how to read this file in the future
                tempFile.WriteByte(69);
                tempFile.WriteByte(74);
                tempFile.WriteByte(66);
                tempFile.WriteByte(65);
                tempFile.WriteByte(69);
                tempFile.WriteByte(83);

                tempFile.Write(iv, 0, iv.Length);

                using (var cryptoStream =
                    new CryptoStream(tempFile, cipher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    fileStream.CopyTo(cryptoStream);
                }
            }

            File.Delete(filePath);
            File.Move(tempFileName, filePath);
        }

        public static void DecryptFile(string filePath, byte[] key)
        {
            string tempFileName = Path.GetTempFileName();

            using (SymmetricAlgorithm cipher = Aes.Create())
            using (FileStream fileStream = File.OpenRead(filePath))
            using (FileStream tempFile = File.Create(tempFileName))
            {
                cipher.Key = key;
                byte[] iv = new byte[cipher.BlockSize / 8];
                byte[] headerBytes = new byte[6];
                int remain = headerBytes.Length;

                while (remain != 0)
                {
                    int read = fileStream.Read(headerBytes, headerBytes.Length - remain, remain);

                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    remain -= read;
                }

                if (headerBytes[0] != 69 ||
                    headerBytes[1] != 74 ||
                    headerBytes[2] != 66 ||
                    headerBytes[3] != 65 ||
                    headerBytes[4] != 69 ||
                    headerBytes[5] != 83)
                {
                    throw new InvalidOperationException();
                }

                remain = iv.Length;

                while (remain != 0)
                {
                    int read = fileStream.Read(iv, iv.Length - remain, remain);

                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    remain -= read;
                }

                cipher.IV = iv;

                using (var cryptoStream =
                    new CryptoStream(tempFile, cipher.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    fileStream.CopyTo(cryptoStream);
                }
            }

            File.Delete(filePath);
            File.Move(tempFileName, filePath);
        }
    }
}
