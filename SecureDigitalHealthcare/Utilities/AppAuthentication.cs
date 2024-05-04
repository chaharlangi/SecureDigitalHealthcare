using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SecureDigitalHealthcare.Models;

namespace SecureDigitalHealthcare.Utilities
{
    public class AppAuthentication
    {
        public static bool IsAuthenticated(HttpContext httpContext)
        {
            if (httpContext.User.Identity is not null)
            {
                return httpContext.User.Identity.IsAuthenticated;
            }

            return false;
        }
        public static async Task SignIn(HttpContext httpContext, int userId, string userName, string role, bool rememberMe)
        {
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Sid, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            ];

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties authProperties = new()
            {
                AllowRefresh = true,
                IsPersistent = rememberMe
            };

            await httpContext.SignInAsync(
                               CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                                             authProperties);
        }

        public static int GetCurrentUserId(ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.Sid).Value);
        }

        public static void SignOut(HttpContext httpContext)
        {
            httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }


        public static bool IsAdmin(HttpContext httpContext)
        {
            return httpContext.User.IsInRole(AppRole.Admin);
        }

        public static bool IsDoctor(HttpContext httpContext)
        {
            return httpContext.User.IsInRole(AppRole.Doctor);
        }

        public static bool IsPatient(HttpContext httpContext)
        {
            return httpContext.User.IsInRole(AppRole.Patient);
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole(AppRole.Admin);
        }
        public static bool IsDoctor(ClaimsPrincipal user)
        {
            return user.IsInRole(AppRole.Doctor);
        }
        public static bool IsPatient(ClaimsPrincipal user)
        {
            return user.IsInRole(AppRole.Patient);
        }

    }
}
