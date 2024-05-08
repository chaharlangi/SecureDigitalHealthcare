using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.Models;
using System.Security.Claims;
using DNTCaptcha.Core;
using System.Security.Principal;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using SecureDigitalHealthcare.Constants;
using SecureDigitalHealthcare.DTOs;
using Microsoft.EntityFrameworkCore;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccountController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        #region Index
        public IActionResult Index()
        {
            if (AppAuthentication.IsPatient(HttpContext))
            {
                return IndexPatient();
            }
            if (AppAuthentication.IsDoctor(HttpContext))
            {
                return IndexDoctor();
            }
            else if (AppAuthentication.IsAdmin(HttpContext))
            {
                return IndexAdmin();
            }

            return View();
        }

        public IActionResult IndexPatient()
        {
            return Content("Patient Dashboard");
        }
        IActionResult IndexDoctor()
        {
            return Content("Doctor Dashboard");
        }
        IActionResult IndexAdmin()
        {
            return Content("Admin Dashboard");
        }
        #endregion

        #region Signup
        public IActionResult Signup()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "It is invalid")]
        [HttpPost, ActionName("Signup")]
        public async Task<IActionResult> ConfirmSignup(User user, IFormFile profilePictureInput, string honeypot = "")
        {
            if (ModelState.IsValid == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = MyHelper.GetErrorListFromModelStateString(ModelState);

                return View();
            }

            if (string.IsNullOrEmpty(honeypot) == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.YouAreABot;
                return View();
            }
            if (_context.Users.FirstOrDefault(x => x.Email == user.Email) is not null)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.EmailAlreadyExists;
                return View();
            }
            if (DateTime.Now - user.BirthDate < TimeSpan.FromDays(365 * 18))
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.YouMustBe18;
                return View();
            }
            if (profilePictureInput is not null && profilePictureInput.Length > 0)
            {
                bool imageSave = FileManager.SaveProfileImage(_environment, profilePictureInput, out string fileName);
                user.ProfileImagePath = fileName;
            }

            user.RoleId = AppRole.GetRoleId(AppRole.Patient);
            user.RegistrationDate = DateTime.Now;
            user.Password = PasswordHasher.HashPassword(user.Password!);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            await AppAuthentication.SignIn(HttpContext, user.Id, user.Name!, AppRole.Patient, true);

            return RedirectToAction("Index", "Home");


        }
        #endregion

        #region Login
        public IActionResult Login()
        {
            if (AppAuthentication.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO newUser)
        {
            if (ModelState.IsValid == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = MyHelper.GetErrorListFromModelStateString(ModelState);
                return View();
            }

            var user = _context.Users.Include(x => x.Role).FirstOrDefault(u => u.Email == newUser.Email);
            if (user is null)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.UserNotFound;
                return View();
            }
            if (PasswordHasher.VerifyPassword(newUser.Password!, user.Password!) == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.WrongPassword;
                return View();
            }

            await AppAuthentication.SignIn(HttpContext, user.Id, user.Name!, user.Role.Name!, newUser.RememberMe);

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Logout
        public IActionResult Logout()
        {
            AppAuthentication.SignOut(HttpContext);

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Utilities
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
        #endregion
    }
}