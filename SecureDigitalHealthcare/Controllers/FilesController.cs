using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class FilesController : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        public FilesController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost]
        public IActionResult UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var encryptedBytes = AppEncryptor.EncryptBytes(fileBytes);

                    var filePath = Path.Combine(MyHelper.ProfilePicturesFolderName, file.FileName);
                    System.IO.File.WriteAllBytes(filePath, encryptedBytes);

                    return Ok("File uploaded and encrypted successfully");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var imagePath = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName, fileName);

            //var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                var decryptedBytes = AppEncryptor.DecryptBytes(imageBytes);

                return File(decryptedBytes, $"application/octet-stream"); // Adjust content type based on your image type
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        public IActionResult HashPassword(string password)
        {
            var hashedPass = AppHasher.HashPassword(password);
            var hashedPassJson = JsonConvert.SerializeObject(hashedPass);
            var passJsonPath = Path.Combine(_environment.GetRootProjectPath(),
                MyHelper.ProfilePicturesFolderName, "password.json");
            System.IO.File.WriteAllText(passJsonPath, hashedPassJson);
            return Ok(hashedPassJson);
        }



        public static IFormFile GetFileLocally(IWebHostEnvironment _environment, string fileName)
        {
            var filePath = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName, fileName);

            byte[] fileBytes = AppEncryptor.DecryptBytes(System.IO.File.ReadAllBytes(filePath));

            return new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, fileName, fileName);
        }
        public static bool StoreProfileImage(IWebHostEnvironment _environment, IFormFile profilePictureInput, out string fileName)
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


            while (System.IO.File.Exists(filePath))
            {
                // If the file already exists, generate a new filename
                fileName = Guid.NewGuid().ToString() + fileExtension;
                filePath = Path.Combine(folder, fileName);
            }

            using (var memoryStream = new MemoryStream())
            {
                profilePictureInput.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var encryptedBytes = AppEncryptor.EncryptBytes(fileBytes);
                System.IO.File.WriteAllBytes(filePath, encryptedBytes);
            }

            return true;
        }
        public static bool DeleteProfileImage(IWebHostEnvironment _environment, string fileName)
        {
            // Store files outside public folders
            var folder = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName);
            var filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }


            return false;
        }

    }
}
