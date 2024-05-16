using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SecureDigitalHealthcare.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Azure;
using Azure.Core;
using System.Security.Cryptography;
using System;
using Microsoft.EntityFrameworkCore;

namespace SecureDigitalHealthcare.Utilities
{
    public class AppAuthentication
    {
        private const string OTPCode = "OTPCode";
        private const string LastOTPCodeSentDate = "LastOTPCodeSentDate";
        private const int OTPCodeLength = 5;
        private const int OTPCodeExpireSeconds = 60 * 2;
        private const int ForgetPasswordTokenExpireSeconds = 60 * 5;
        private const int ForgetPasswordTokenLength = 32;

        public static void SetExpiryOTPCode(HttpContext httpContext, string otp)
        {
            otp = AppEncryptor.EncryptString(otp);

            DeleteCookie(httpContext, OTPCode);
            httpContext.Response.Cookies.Append(OTPCode, otp, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict, // Helps prevent CSRF attacks
                Expires = DateTimeOffset.UtcNow.AddSeconds(OTPCodeExpireSeconds)
            });

            DeleteCookie(httpContext, LastOTPCodeSentDate);
            httpContext.Response.Cookies.Append(LastOTPCodeSentDate, DateTime.UtcNow.AddSeconds(OTPCodeExpireSeconds).ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict, // Helps prevent CSRF attacks
                Expires = DateTimeOffset.UtcNow.AddSeconds(OTPCodeExpireSeconds)
            });
        }
        public static string GetOTPCode(HttpContext httpContext)
        {
            return AppEncryptor.DecryptString(httpContext.Request.Cookies[OTPCode]!);
        }
        public static DateTime? GetLastTimeOTPSent(HttpContext httpContext)
        {
            var dateTimeString = httpContext.Request.Cookies[LastOTPCodeSentDate];
            if (dateTimeString is not null)
            {
                return DateTime.Parse(dateTimeString);
            }
            return null;
        }
        public static string GenerateOTPCode()
        {
            string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, OTPCodeLength).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static bool CanSendOTP(HttpContext httpContext)
        {
            DateTime? lastTime = GetLastTimeOTPSent(httpContext);
            if (lastTime == null)
            {
                return true;
            }
            var totalSeconds = (DateTime.UtcNow - lastTime).Value.TotalSeconds;
            return totalSeconds > OTPCodeExpireSeconds;
        }

        public static void DeleteCookie(HttpContext httpContext, string keyCookie)
        {
            var cookiesToDelete = httpContext.Request.Cookies.Where(key => key.Equals(keyCookie)).ToList();

            foreach (var cookie in cookiesToDelete)
            {
                httpContext.Response.Cookies.Delete(cookie.Key);
                AppDebug.Log($"{cookie.Key}: {cookie.Value} => Deleted!");
            }
        }

        public static bool CanSendForgetPasswordLink(EasyHealthContext context, string email)
        {
            var user = context.Users.Include(x => x.ForgetPasswordToken).FirstOrDefault(u => u.Email == email);
            if (user is not null)
            {
                var forgetPasswordToken = user.ForgetPasswordToken;
                if (forgetPasswordToken is not null)
                {
                    var totalSeconds = (DateTime.UtcNow - forgetPasswordToken.ExpirationDate).TotalSeconds;
                    return totalSeconds > ForgetPasswordTokenExpireSeconds;
                }
                return true;
            }
            return false;
        }
        public static async Task<bool> SetExpiryForgetPasswordToken(EasyHealthContext context, string email, string token)
        {
            var user = context.Users.Include(x => x.ForgetPasswordToken).FirstOrDefault(u => u.Email == email);
            if (user is not null)
            {
                ForgetPasswordToken forgetPasswordToken = new()
                {
                    Token = AppEncryptor.EncryptString(token),
                    ExpirationDate = DateTime.UtcNow.AddSeconds(ForgetPasswordTokenExpireSeconds)
                };

                user.ForgetPasswordToken = forgetPasswordToken;

                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public static async Task<bool> DeleteForgetPasswordToken(EasyHealthContext context, string email)
        {
            var user = context.Users.Include(x => x.ForgetPasswordToken).FirstOrDefault(u => u.Email == email);
            if (user is not null && user.ForgetPasswordToken is not null)
            {
                context.ForgetPasswordTokens.Remove(user.ForgetPasswordToken);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public static bool VerifyUserForgetPasswordToken(EasyHealthContext context, string email, string token)
        {
            var user = context.Users.Include(x => x.ForgetPasswordToken).FirstOrDefault(u => u.Email == email);
            if (user is not null)
            {
                var forgetPasswordToken = user.ForgetPasswordToken;
                if (forgetPasswordToken is not null)
                {
                    AppDebug.Log($"Input: {token}");
                    AppDebug.Log($"Stored: {(forgetPasswordToken.Token)}");
                    AppDebug.Log($"Stored decrypted: {AppEncryptor.DecryptString(forgetPasswordToken.Token)}");
                    return AppEncryptor.DecryptString(forgetPasswordToken.Token) == token;
                }
            }
            return false;
        }
        public static bool IsUserForgetPasswordTokenValid(EasyHealthContext context, string email)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user is not null)
            {
                var forgetPasswordToken = user.ForgetPasswordToken;
                if (forgetPasswordToken is not null)
                {
                    return forgetPasswordToken.ExpirationDate > DateTime.UtcNow;
                }
            }
            return true;
        }
        public static string GenerateForgetPasswordToken()
        {
            byte[] randomBytes = new byte[ForgetPasswordTokenLength];
            using (var rngProvider = RandomNumberGenerator.Create())
            {
                rngProvider.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
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
        public static void SignOut(HttpContext httpContext)
        {
            httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public static bool IsAuthenticated(HttpContext httpContext)
        {
            if (httpContext.User.Identity is not null)
            {
                return httpContext.User.Identity.IsAuthenticated;
            }

            return false;
        }
        public static int GetCurrentUserId(ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.Sid).Value);
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
