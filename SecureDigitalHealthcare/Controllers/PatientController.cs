using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;

namespace SecureDigitalHealthcare.Controllers
{
    [Authorize(Roles = AppRole.Patient)]
    public class PatientController : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        public PatientController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return Content("Patient Controller");
            return View();
        }

        public IActionResult BookAppointment()
        {
            //var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).ToList();
            //List<DoctorsViewModel> doctorsList = new();
            //foreach (var doctor in doctors)
            //{
            //    doctorsList.Add(new DoctorsViewModel
            //    {
            //        Name = doctor.IdNavigation.Name,
            //        LastName = doctor.IdNavigation.LastName,
            //        Speciality = doctor.Speciality.Name
            //    });
            //}
            //return View("DoctorsList", doctorsList);
            return View();
        }

    }
}
