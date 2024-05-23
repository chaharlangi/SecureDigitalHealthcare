using SecureDigitalHealthcare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
using SecureDigitalHealthcare.DTOs;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using System;
using System.Numerics;
using System.Security.Claims;
using SecureDigitalHealthcare.Utilities.Communication;
using static SecureDigitalHealthcare.Utilities.Communication.RoomCallManager;

namespace SecureDigitalHealthcare.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly EasyHealthContext _context;

        public AppointmentsController(EasyHealthContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = PolicyConstants.MustBePatient)]
        public IActionResult BookAppointment(int doctorId)
        {

            var doctor = _context.Doctors.
                Include(d => d.Speciality).
                Include(x => x.Availabilities).
                Include(x => x.IdNavigation.CommentSenders).ThenInclude(c => c.Receiver).
                Include(x => x.IdNavigation.CommentReceivers).ThenInclude(c => c.Sender).
                Include(d => d.IdNavigation).
                FirstOrDefault(x => x.Id == doctorId);

            DoctorAppointmentDTO doctorAppointmentDTO = new(doctor!);

            return View(doctorAppointmentDTO);
        }

        [ValidateAntiForgeryToken]
        [Authorize(Policy = PolicyConstants.MustBePatient)]
        [HttpPost]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime startTime, DateTime endTime, bool taken)
        {
            //return Content($"{doctorId}, {startTime}, {endTime}, {taken}");
            Availability availability = new Availability()
            {
                DoctorId = doctorId,
                StartTime = startTime,
                EndTime = endTime,
                Taken = taken
            };

            var selectedAvailablity = _context.Availabilities.FirstOrDefault(x => x.DoctorId == availability.DoctorId && x.StartTime == availability.StartTime && x.EndTime == availability.EndTime);

            selectedAvailablity!.Taken = true;

            RoomCallData roomCall = await RoomCallManager.CreateRoomAsync();

            var timeSpan = availability.EndTime - availability.StartTime;
            var appontment = new Appointment()
            {
                DoctorId = availability.DoctorId,
                PatientId = int.Parse(User.FindFirst(ClaimTypes.Sid)!.Value),
                AvailabilityId = selectedAvailablity.Id,
                Accepted = false,
                Done = false,
                RoomCall = new RoomCall()
                {
                    Id = roomCall.RoomId,
                    HostId = roomCall.HostId,
                    GuestId = roomCall.GuestId,
                    HostAccessToken = roomCall.HostAccessToken,
                    GuestAccessToken = roomCall.GuestAccessToken,
                }
            };

            _context.Appointments.Add(appontment);

            await _context.SaveChangesAsync();

            var doctor = _context.Doctors.Include(d => d.Speciality).Include(d => d.IdNavigation).Include(x => x.Availabilities).FirstOrDefault(x => x.Id == availability.DoctorId);
            return RedirectToAction(nameof(BookAppointment), new { doctorId = doctorId });
        }
        [Authorize(Policy = PolicyConstants.MustBeDoctor)]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            //foreach (var item in HttpContext.User.Identities)
            //{
            //    AppDebug.Log($"{item.NameClaimType}, {item.RoleClaimType}, {item.Name}");
            //};
            //return Content($"{HttpContext.User.Claims}");

            var appointments = await _context.Appointments
                                .Include(x => x.Patient)
                                .Include(x => x.Doctor)
                                .Include(x => x.Doctor.IdNavigation)
                                .Include(x => x.Doctor.Speciality)
                                .Include(x => x.Availability)
                                .Include(x => x.RoomCall)
                                .Where(x => x.DoctorId == AppAuthentication.GetCurrentUserId(User))
                                .ToListAsync();

            ViewBag.VideoCallUrl = VideoCallController.GetVideoCallUrl();

            return View(appointments);
        }

        [Authorize(Policy = PolicyConstants.MustBePatient)]
        public async Task<IActionResult> GetPatientAppointments()
        {
            var appointments = await _context.Appointments
                                .Include(x => x.Doctor)
                                .Include(x => x.Doctor.IdNavigation)
                                .Include(x => x.Doctor.Speciality)
                                .Include(x => x.RoomCall)
                                .Include(x => x.Availability)
                                .Where(x => x.PatientId == AppAuthentication.GetCurrentUserId(User))
                                .ToListAsync();

            ViewBag.VideoCallUrl = VideoCallController.GetVideoCallUrl();

            return View(appointments);
        }

        [Authorize(Policy = PolicyConstants.MustBeDoctorOrPatient)]
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(CancelAppointmentDTO cancelAppointment)
        {
            var appointment = await _context.Appointments
                .Include(x => x.Availability)
                .Include(x => x.RoomCall)
                .FirstOrDefaultAsync(x => x.DoctorId == cancelAppointment.DoctorId && x.PatientId == cancelAppointment.PatientId && x.AvailabilityId == cancelAppointment.AvailabilityId);

            //return Content($"DoctorId: {cancelAppointment.DoctorId}, PatientId: {cancelAppointment.PatientId}, AvailabilityId: {cancelAppointment.AvailabilityId}");

            appointment.Availability.Taken = false;

            _context.Appointments.Remove(appointment);
            _context.RoomCalls.Remove(appointment.RoomCall);

            await _context.SaveChangesAsync();

            await RoomCallManager.DeleteRoom(appointment.RoomCall.Id);

            return AppAuthentication.IsDoctor(User) ? RedirectToAction(nameof(GetDoctorAppointments)) : RedirectToAction(nameof(GetPatientAppointments));
        }
    }
}
