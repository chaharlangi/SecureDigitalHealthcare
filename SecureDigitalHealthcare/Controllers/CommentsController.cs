using Azure.Core;
using SecureDigitalHealthcare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using System.Security.Claims;

namespace SecureDigitalHealthcare.Controllers
{
    public class CommentsController : Controller
    {
        public readonly EasyHealthContext _context;
        public CommentsController(EasyHealthContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = AppRole.Patient)]
        public async Task<IActionResult> SubmitCommentToDoctor(int receiverId, int? replayToId, string textComment)
        {
            //return Json(MyHelper.GetErrorListFromModelStateString(ModelState));
            Comment comment = new Comment()
            {
                SenderId = AppAuthentication.GetCurrentUserId(User),
                ReceiverId = receiverId,
                ReplyTo = replayToId,
                Text = textComment,
                Date = DateTime.Now
            };
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction("BookAppointment", "Appointments", new { doctorId = receiverId });
        }
    }
}
