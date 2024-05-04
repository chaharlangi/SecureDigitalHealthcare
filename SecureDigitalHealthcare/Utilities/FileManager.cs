using EasyHealth;

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

    }
}
