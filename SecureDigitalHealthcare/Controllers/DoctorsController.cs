using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using SecureDigitalHealthcare.DTOs;
using Humanizer;
using NuGet.Protocol.Plugins;

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
        public IActionResult GetDoctorsByName(string doctorName)
        {
            if (string.IsNullOrEmpty(doctorName))
            {
                return RedirectToAction(nameof(GetAllDoctors));
            }

            var doctors = _context.Doctors
                .Include(d => d.IdNavigation)
                .Where(d => d.IdNavigation.Name.Contains(doctorName) || d.IdNavigation.LastName.Contains(doctorName))
                .Include(d => d.Speciality).ToList();


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
            Doctor doctor = _context.Doctors.Include(d => d.Appointments).Include(d => d.Availabilities).FirstOrDefault(d => d.Id == AppAuthentication.GetCurrentUserId(User))!;
            //var availabilities = doctor.Availabilities.Where(x => x.StartTime.Date >= DateTime.Now.Date);
            var availabilities = doctor.Availabilities;

            return View(availabilities);
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
                ViewData[ViewDataConstants.ValidationMessage] = "The availabilities already exists!";
            }


            return View(nameof(GetDoctorAvailabilites), doctor.Availabilities);
        }
        [Authorize(Policy = PolicyConstants.MustBeDoctor)]
        [HttpPost]
        public async Task<IActionResult> AddPredefinedAvailibility(PredefinedAvailability predefinedAvailability)
        {
            var year = predefinedAvailability.date.Year;
            var month = predefinedAvailability.date.Month;
            var day = predefinedAvailability.date.Day;

            var from = new DateTime(year, month, day, predefinedAvailability.from.Hour, predefinedAvailability.date.Minute, predefinedAvailability.date.Second);
            var to = new DateTime(year, month, day, predefinedAvailability.to.Hour, predefinedAvailability.to.Minute, predefinedAvailability.to.Second);

            if (from > to)
            {
                ViewData[ViewDataConstants.ValidationMessage] = "The start time is greater than the end time!";
                return View(nameof(GetDoctorAvailabilites), predefinedAvailability);
            }

            var doctor = await _context.Doctors.Include(d => d.Availabilities).FirstOrDefaultAsync(x => x.Id == AppAuthentication.GetCurrentUserId(User));

            DateTime currentStart = from;

            while (currentStart.AddMinutes(predefinedAvailability.minutes) <= to)
            {
                DateTime currentEnd = currentStart.AddMinutes(predefinedAvailability.minutes);

                if (currentEnd > to)
                {
                    currentEnd = to;
                }

                var canAddAvailability = _context.Availabilities
                    .Any(x => x.DoctorId == doctor.Id &&
                        (x.StartTime <= currentStart && x.EndTime >= currentStart)) == false;

                if (canAddAvailability)
                {
                    doctor.Availabilities.Add(new Availability
                    {
                        DoctorId = doctor.Id,
                        StartTime = currentStart,
                        EndTime = currentEnd,
                        Taken = false
                    });
                }
                else
                {
                    ViewData[ViewDataConstants.ValidationMessage] = "One of the availabilities already exists!";

                    return View(nameof(GetDoctorAvailabilites), doctor.Availabilities);
                }

                currentStart = currentEnd;
            }

            await _context.SaveChangesAsync();


            return View(nameof(GetDoctorAvailabilites), doctor.Availabilities);
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
