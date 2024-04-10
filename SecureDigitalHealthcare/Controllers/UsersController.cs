using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SecureDigitalHealthcare.Models;

namespace SecureDigitalHealthcare.Controllers
{
    public class UsersController : Controller
    {
        private readonly SecureDigitalHealthcareContext _context;
        private readonly IWebHostEnvironment _environment;

        public UsersController(SecureDigitalHealthcareContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            //return View("CustomUser");
            return View(await _context.Users.ToListAsync());
        }

        public IActionResult Terms()
        {
            return Content("Terms are yet to be written! For now, just check the box");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.NationalId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "It is invalid")]
        public async Task<IActionResult> Create(User user, IFormFile profilePictureInput, string honeypot = "")
        {
            if (string.IsNullOrEmpty(honeypot) == false)
            {
                return Json("You are a bot!");
                //return RedirectToAction(nameof(Index));
            }

            if (profilePictureInput is not null && profilePictureInput.Length > 0)
            {
                bool imageSave = SaveProfileImage(profilePictureInput, out string fileName);

                if (ModelState.IsValid)
                {
                    user.ProfileImagePath = fileName;
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(user);
        }

        private bool SaveProfileImage(IFormFile profilePictureInput, out string fileName)
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
            var folder = Path.Combine(_environment.ContentRootPath, "ProfilePictures");
            var filePath = Path.Combine(folder, fileName);

            while (System.IO.File.Exists(filePath))
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
        private bool DeleteProfileImage(string fileName)
        {
            // Store files outside public folders
            var folder = Path.Combine(_environment.ContentRootPath, "ProfilePictures");
            var filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }


            return false;
        }

        public IActionResult GetImage(string imageName)
        {
            var imagePath = Path.Combine(_environment.ContentRootPath, "ProfilePictures", imageName);

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
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "It is invalid")]
        public async Task<IActionResult> Edit(string id, User user, IFormFile profilePictureInput)
        {
            if (id != user.NationalId)
            {
                return NotFound();
            }

            //return Json(MyHelper.GetErrorListFromModelState(ModelState));

            if (ModelState.IsValid)
            {
                try
                {
                    var fileToDelete = _context.Users.AsNoTracking().FirstOrDefault(x => x.NationalId == id)?.ProfileImagePath;
                    DeleteProfileImage(fileToDelete);
                    SaveProfileImage(profilePictureInput, out string fileName);

                    user.ProfileImagePath = fileName;

                    //user.ProfileImagePath = "";

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.NationalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.NationalId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.NationalId == id);
        }
    }
}
