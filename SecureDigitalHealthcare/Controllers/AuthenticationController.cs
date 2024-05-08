using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        public readonly IConfiguration _config;

        public AuthenticationController(IConfiguration config)
        {
            _config = config;
        }

        public record AuthenticationData(string? UserName, string? Password);
        public record UserData(int UserId, string UserName);


        [HttpPost, ActionName("token")]
        public ActionResult<string> Authenticate([FromForm] AuthenticationData data)
        {
            //return Json(data);
            var user = ValidateCredentials(data);

            if (user is null)
            {
                return Content("You are could not authenticate");
            }

            var token = GenerateToken(user);

            return Ok(token);

        }

        private string GenerateToken(UserData user)
        {

            var secretKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_config.GetValue<string>("Authentication:SecretKey")!));

            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName.ToString())
            };

            var token = new JwtSecurityToken(
                               issuer: _config.GetValue<string>("Authentication:Issuer"),
                               audience: _config.GetValue<string>("Authentication:Audience"),
                               claims: claims,
                               notBefore: DateTime.UtcNow,
                               expires: DateTime.Now.AddMinutes(1),
                               signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private UserData? ValidateCredentials(AuthenticationData data)
        {
            if (CompareValues(data.UserName, "admin")
                && CompareValues(data.Password, "admin"))
            {
                return new UserData(1, data.UserName!);
            }

            if (CompareValues(data.UserName, "sia")
                && CompareValues(data.Password, "sia"))
            {
                return new UserData(2, data.UserName!);
            }

            return null;
        }

        private bool CompareValues(string? actual, string expected)
        {
            if (actual is not null)
            {
                if (actual.Equals(expected))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
