using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.DTOs;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class VideoCallController : Controller
    {
        public IActionResult Index()
        {
            return View(new RoomCallDTO()
            {
                UserId = "123",
                RoomId = "456",
                AccessToken = "789"
            });
        }
    }
}
