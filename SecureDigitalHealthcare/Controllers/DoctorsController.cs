using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using SecureDigitalHealthcare.DTOs;

namespace SecureDigitalHealthcare.Controllers
{
    public class DoctorsController : Controller
    {
        public const string ViewDoctorsListToBook = "DoctorsListToBook";

        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        public DoctorsController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = AppRole.Patient)]
        public IActionResult GetAllDoctors()
        {
            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).ToList();

            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }
        [Authorize(Roles = AppRole.Patient)]
        public IActionResult GetDoctorsByLastName(string lastName)
        {
            if (string.IsNullOrEmpty(lastName))
            {
                return RedirectToAction(nameof(GetAllDoctors));
            }

            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).Where(d => d.IdNavigation.LastName.Contains(lastName)).ToList();


            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }
        [Authorize(Roles = AppRole.Patient)]
        public IActionResult GetDoctorsBySpeciality(string speciality)
        {
            if (string.IsNullOrEmpty(speciality))
            {
                return RedirectToAction(nameof(GetAllDoctors));
            }

            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).Where(d => d.Speciality.Name.Contains(speciality)).ToList();

            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }

        public static List<DoctorDTO> GetListsByDoctors(List<Doctor> doctors)
        {
            List<DoctorDTO> doctorsList = new();
            foreach (var doctor in doctors)
            {
                doctorsList.Add(new(doctor.Id,
                                    doctor.IdNavigation.Name!,
                                    doctor.IdNavigation.LastName!,
                                    doctor.Speciality!.Name!,
                                    doctor.IdNavigation.ProfileImagePath!));
            }
            return doctorsList;
        }
    }
}
