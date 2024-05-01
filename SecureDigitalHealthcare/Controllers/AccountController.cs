using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.Models;
using System.Security.Claims;

namespace SecureDigitalHealthcare.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (ModelState.IsValid == false)
            {
                return Content("Invalid model state");
                //return View("Index", user);
            }

            if (user.Username == "admin" && user.Password == "admin")
            {

                List<Claim> claims = new()
            {
                new Claim("UserName", user.Username!)
            };

                ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties authProperties = new()
                {
                    AllowRefresh = true,
                    IsPersistent = user.RememberMe
                };

                await HttpContext.SignInAsync(
                                   CookieAuthenticationDefaults.AuthenticationScheme,
                                                  new ClaimsPrincipal(claimsIdentity),
                                                                 authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ValidationMessage"] = "Invalid username or password";

            return View();
        }
    }
}
