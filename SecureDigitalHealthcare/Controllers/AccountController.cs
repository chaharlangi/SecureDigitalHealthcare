using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.Models;
using DNTCaptcha.Core;
using SecureDigitalHealthcare.Utilities;
using SecureDigitalHealthcare.Constants;
using SecureDigitalHealthcare.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Imaging;
using SecureDigitalHealthcare.Utilities.Communication;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.IdentityModel.Logging;
using System;

namespace SecureDigitalHealthcare.Controllers
{
    public class AccountController : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        private ChangePasswordDTO EmptyChangePasswordDTO
        {
            get => new ChangePasswordDTO()
            {
                Email = AppAuthentication.GetCurrentUserEmail(User),
                IsResetLink = false,
                OTP = "",
                Password = ""
            };
        }

        public AccountController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        #region Index

        [AllowAnonymous]
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

        [AllowAnonymous]
        public IActionResult IndexPatient()
        {
            return Content("Patient Dashboard");
        }

        [AllowAnonymous]
        IActionResult IndexDoctor()
        {
            return Content("Doctor Dashboard");
        }

        [AllowAnonymous]
        IActionResult IndexAdmin()
        {
            return Content("Admin Dashboard");
        }
        #endregion

        #region Signup

        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "It is invalid")]
        [HttpPost]
        public async Task<IActionResult> Signup(User user, IFormFile profilePictureInput = null, string honeypot = "")
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

            user.ProfileImagePath = HandleNewProfilePicture(_environment, profilePictureInput);
            await AddNewUser(_context, user);

            await AppAuthentication.SignIn(HttpContext, user.Id, user.Name!, user.Email, AppRole.Patient, true);

            return RedirectToAction("Index", "Home");


        }

        public static async Task<User> AddNewUser(EasyHealthContext context, User user)
        {
            user.RoleId = AppRole.GetRoleId(AppRole.Patient);
            user.RegistrationDate = DateTime.Now;
            user.Password = AppHasher.HashPassword(user.Password!);

            var addedUser = context.Users.Add(user);

            await context.SaveChangesAsync();

            return addedUser.Entity;
        }
        #endregion

        #region Login

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (AppAuthentication.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO newUser)
        {
            if (ModelState.IsValid == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = MyHelper.GetErrorListFromModelStateString(ModelState);
                return View();
            }

            if (IsAdmin(newUser.Email, newUser.Password))
            {
                await AppAuthentication.SignIn(HttpContext, -1, "admin", newUser.Email, AppRole.Admin, newUser.RememberMe);

                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.Include(x => x.Role).FirstOrDefault(u => u.Email == newUser.Email);
            if (user is null)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.UserNotFound;
                return View();
            }
            if (AppHasher.VerifyPassword(newUser.Password, user.Password) == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.WrongPassword;
                return View();
            }

            await AppAuthentication.SignIn(HttpContext, user.Id, user.Name!, user.Email, user.Role.Name!, newUser.RememberMe);

            return RedirectToAction("Index", "Home");
        }

        private bool IsAdmin(string email, string password)
        {
            return email == "admin@gmail.com" && password == "admin";
        }
        #endregion

        #region Logout
        [Authorize]
        public IActionResult Logout()
        {
            AppAuthentication.SignOut(HttpContext);

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Edit Profile
        [Authorize]
        public IActionResult EditProfile()
        {
            var user = _context.Users.FirstOrDefault(x => x.Id.Equals(AppAuthentication.GetCurrentUserId(User)));

            EditProfileDTO editProfileDTO = new EditProfileDTO()
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Address = user.Address,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                ProfileImagePath = user.ProfileImagePath
            };

            return View(editProfileDTO);
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileDTO editProfileDTO, IFormFile profilePictureInput = null)
        {
            User user = _context.Users.FirstOrDefault(x => x.Id == editProfileDTO.Id);

            user.Name = editProfileDTO.Name;
            user.LastName = editProfileDTO.LastName;
            user.Address = editProfileDTO.Address;
            user.BirthDate = editProfileDTO.BirthDate;
            user.PhoneNumber = editProfileDTO.PhoneNumber;

            string newProfilePictureName = user.ProfileImagePath;
            if (profilePictureInput != null)
            {
                HandlePreviousProfilePicture(_environment, user.ProfileImagePath);
                newProfilePictureName = HandleNewProfilePicture(_environment, profilePictureInput);
            }

            user.ProfileImagePath = newProfilePictureName;
            editProfileDTO.ProfileImagePath = newProfilePictureName;

            await _context.SaveChangesAsync();

            return View(editProfileDTO);
        }
        #endregion

        #region ChangePassword
        [AllowAnonymous]
        public IActionResult ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (string.IsNullOrEmpty(changePasswordDTO.OTP))
            //if (changePasswordDTO is null)
            {
                AppDebug.Log("inja");

                return View(EmptyChangePasswordDTO);
            }
            AppDebug.Log("ounja");

            changePasswordDTO.IsResetLink = true;

            return View(changePasswordDTO);
        }
        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendOTP(string email)
        {
            AppDebug.Log(email);
            if (string.IsNullOrEmpty(email))
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Please input your email!";
                return View(nameof(ChangePassword), EmptyChangePasswordDTO);
            }
            if (AppAuthentication.CanSendOTP(HttpContext) == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"You must wait a while for sending the otp again. It is already sent!";
                return View(nameof(ChangePassword), EmptyChangePasswordDTO);
            }
            string generatedOtp = AppAuthentication.GenerateOTPCode();
            AppAuthentication.SetExpiryOTPCode(HttpContext, generatedOtp);

            await SendEmail(email, "EasyHealth: OTP", generatedOtp);

            ViewData[ViewDataConstants.ValidationMessage] = $"Please check your email for OTP";
            return View(nameof(ChangePassword), EmptyChangePasswordDTO);
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendForgetPasswordLinkPassword(string email)
        {
            if (AppAuthentication.CanSendForgetPasswordLink(_context, email) == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"You must wait a while for sending the reset link again. It is already sent!";
                return View(nameof(ForgetPassword));
            }

            string generatedToken = AppAuthentication.GenerateForgetPasswordToken();
            bool tokenIsSet = await AppAuthentication.SetExpiryForgetPasswordToken(_context, email, generatedToken);

            if (tokenIsSet == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Something went wrong with token saving! Try agian";
                return View(nameof(ForgetPassword));
            }
            ChangePasswordDTO changePasswordDTO = new ChangePasswordDTO()
            {
                Email = email,
                OTP = generatedToken
            };
            var resetUrl = Url.Action(nameof(ChangePassword), nameof(AccountController).Replace("Controller", ""),
                new
                {
                    Email = changePasswordDTO.Email,
                    OTP = changePasswordDTO.OTP
                }, HttpContext.Request.Scheme);

            string emailBody = $"Click <a href=\"{resetUrl}\">here</a> to reset your password.";

            //
            bool emailSent = await SendEmail(email, "EasyHealth: Reset Password Link", emailBody);

            if (emailSent == false)
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Something went wrong with email sending! Try agian";
                return View(nameof(ForgetPassword));
            }

            ViewData[ViewDataConstants.ValidationMessage] = $"Check your email for the reset link!";
            return View(nameof(ForgetPassword));
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitNewPasswordOTP(string password, string otp)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(otp))
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Please fill all the fields!";
                return View(nameof(ChangePassword), EmptyChangePasswordDTO);
            }
            AppDebug.Log(otp);
            if (AppAuthentication.GetOTPCode(HttpContext) == otp)
            {
                bool changedPassword = await SetPassword(_context, AppAuthentication.GetCurrentUserId(User), password);

                ViewData[ViewDataConstants.ValidationMessage] = $"Password changed successfully! You can login now";
                return View(nameof(Login));
            }
            else
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Wrong OTP! Try again";
                return View(nameof(ChangePassword), EmptyChangePasswordDTO);
            }
        }
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SubmitNewPasswordForget(string email, string password, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(otp))
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Please fill all the fields!";
                return View(nameof(ChangePassword), EmptyChangePasswordDTO);
            }

            if (AppAuthentication.VerifyUserForgetPasswordToken(_context, email, otp))
            {
                bool changedPassword = await SetPassword(_context, email, password);
                if (changedPassword)
                {
                    await AppAuthentication.DeleteForgetPasswordToken(_context, email);

                    ViewData[ViewDataConstants.ValidationMessage] = $"Password changed successfully! You can login now";
                    return View(nameof(Login));
                }

                return Content($"Could not submit the new password");
            }
            else
            {
                ViewData[ViewDataConstants.ValidationMessage] = $"Wrong Token! Try again";
                ChangePasswordDTO model = new ChangePasswordDTO()
                {
                    Email = email,
                    OTP = otp
                };
                return View(nameof(ChangePassword), model);
            }
        }

        #endregion








        public static string HandleNewProfilePicture(IWebHostEnvironment environment, IFormFile profilePictureInput)
        {
            if (profilePictureInput is null)
            {
                profilePictureInput = FilesController.GetFileLocally(environment, "Default.jpg");
            }

            bool imageSave = FilesController.StoreProfileImage(environment, profilePictureInput, out string fileName);

            return fileName;
        }
        public static void HandlePreviousProfilePicture(IWebHostEnvironment environment, string profilePictureName)
        {
            if (string.IsNullOrEmpty(profilePictureName))
            {
                return;
            }
            FilesController.DeleteProfileImage(environment, profilePictureName);
        }

        private async Task<bool> SendEmail(string email, string subject, string message)
        {

            AppDebug.Log($"{message}");

            IAppSender sender = new AppEmailSender().Setup(email, subject, message);

            return await sender.Send();

            return true;
        }
        private async Task<bool> SetPassword(EasyHealthContext context, string email, string newPassword)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user is not null)
            {
                user.Password = AppHasher.HashPassword(newPassword);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        private async Task<bool> SetPassword(EasyHealthContext context, int userId, string newPassword)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user is not null)
            {
                user.Password = AppHasher.HashPassword(newPassword);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

    }
}