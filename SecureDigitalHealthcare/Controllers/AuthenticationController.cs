using Microsoft.AspNetCore.Mvc;

namespace SecureDigitalHealthcare.Controllers
{
    public class AuthenticationController : Controller
    {
        public record AuthenticationData(string? UserName, string? Password);
        public record UserData(string UserId, string UserName);

        [HttpPost("token")]
        public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
        {

            return Unauthorized();
        }

        private UserData

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
