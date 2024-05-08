using SecureDigitalHealthcare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class ImageController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public ImageController(IWebHostEnvironment webHostEnvironment)
        {
            _environment = webHostEnvironment;
        }

        public IActionResult GetImage(string imageName)
        {
            var imagePath = Path.Combine(_environment.GetRootProjectPath(), MyHelper.ProfilePicturesFolderName, imageName);

            var fileExtension = Path.GetExtension(imageName).ToLowerInvariant();

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{fileExtension}"); // Adjust content type based on your image type
            }
            else
            {
                return NotFound();
            }
        }
    }
}
