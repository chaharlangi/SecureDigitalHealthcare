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

        //32byte key
        const string encryptionKey = "7RBOwUpx1cv7VR+Bi3tWyI+QkWwC5NN7";

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

                    var encryptedBytes = AppAES.EncryptBytes(fileBytes, encryptionKey);

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
            try
            {
                var encryptedFilePath = Path.Combine(MyHelper.ProfilePicturesFolderName, fileName);
                var encryptedBytes = System.IO.File.ReadAllBytes(encryptedFilePath);

                var decryptedBytes = AppAES.DecryptBytes(encryptedBytes, encryptionKey);

                return File(decryptedBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
    }
}
