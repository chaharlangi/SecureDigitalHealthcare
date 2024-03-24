using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Models;

namespace SecureDigitalHealthcare.Controllers
{
    public class TestsControllers : Controller
    {
        public IActionResult Index()
        {
            return Content("HI tests");
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create(int id)
        {

            return Content(id.ToString());
        }
    }
}
