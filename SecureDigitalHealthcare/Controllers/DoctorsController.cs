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

        [Authorize(Policy = PolicyConstants.MustBePatient)]
        public IActionResult GetAllDoctors()
        {
            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).ToList();

            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }
        [Authorize(Policy = PolicyConstants.MustBePatient)]
        public IActionResult GetDoctorsByLastName(string lastName)
        {
            if (string.IsNullOrEmpty(lastName))
            {
                return RedirectToAction(nameof(GetAllDoctors));
            }

            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).Where(d => d.IdNavigation.LastName.Contains(lastName)).ToList();


            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }
        [Authorize(Policy = PolicyConstants.MustBePatient)]
        public IActionResult GetDoctorsBySpeciality(string speciality)
        {
            if (string.IsNullOrEmpty(speciality))
            {
                return RedirectToAction(nameof(GetAllDoctors));
            }

            var doctors = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).Where(d => d.Speciality.Name.Contains(speciality)).ToList();

            return View(ViewDoctorsListToBook, DoctorsController.GetListsByDoctors(doctors));
        }

        [Authorize(Policy = PolicyConstants.MustBeDoctor)]
        public IActionResult GetDoctorAvailabilites()
        {
            Doctor doctor = _context.Doctors.Include(d => d.Availabilities).FirstOrDefault(d => d.Id == AppAuthentication.GetCurrentUserId(User));

            return View(doctor.Availabilities.Where(x => x.StartTime.Date >= DateTime.Now.Date));
        }

        [Authorize(Policy = PolicyConstants.MustBeDoctor)]
        [HttpPost]
        public async Task<IActionResult> DeleteAvailibility(AvailabilityDTO deleteAvailabilityDTO)
        {
            var availability = _context.Availabilities
                .FirstOrDefault(x => x.DoctorId == deleteAvailabilityDTO.DoctorId && x.StartTime == deleteAvailabilityDTO.StartTime && x.EndTime == deleteAvailabilityDTO.EndTime);

            if (availability != null)
            {
                _context.Remove(availability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(GetDoctorAvailabilites));
        }
        [Authorize(Policy = PolicyConstants.MustBeDoctor)]
        [HttpPost]
        public async Task<IActionResult> AddAvailibility(AvailabilityDTO addAvailabilityDTO)
        {
            var canAddAvailability = _context.Availabilities
                .Any(x => x.DoctorId == addAvailabilityDTO.DoctorId &&
                    (x.StartTime <= addAvailabilityDTO.StartTime && x.EndTime >= addAvailabilityDTO.StartTime)) == false;

            //foreach (var item in _context.Availabilities)
            //{
            //    bool conflict = (item.StartTime <= addAvailabilityDTO.StartTime && item.EndTime >= addAvailabilityDTO.StartTime);
            //    string result = "\n";
            //    result += $"Proposed:\t{addAvailabilityDTO.StartTime}\n";
            //    result += $"Start:\t{item.StartTime} < (={item.StartTime <= addAvailabilityDTO.StartTime}=)\n";
            //    result += $"End:\t{item.EndTime} > (={item.EndTime >= addAvailabilityDTO.StartTime}=)\n";
            //    result += $"Conflicts:\t{conflict}\n";
            //    AppDebug.Log($"{result}");
            //}
            //return Content(canAddAvailability.ToString());
            var doctor = await _context.Doctors.Include(d => d.Availabilities).FirstOrDefaultAsync(d => d.Id == addAvailabilityDTO.DoctorId);

            if (canAddAvailability == true)
            {
                doctor.Availabilities.Add(new Availability
                {
                    DoctorId = addAvailabilityDTO.DoctorId,
                    StartTime = addAvailabilityDTO.StartTime,
                    EndTime = addAvailabilityDTO.EndTime,
                    Taken = false
                });
                //_context.Availabilities.Add(newSlot);
                await _context.SaveChangesAsync();
            }
            else
            {
                AppDebug.Log("Availability already exists");
            }

            ViewData[ViewDataConstants.ValidationMessage] = "This availability already exists";

            return RedirectToAction(nameof(GetDoctorAvailabilites));
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
