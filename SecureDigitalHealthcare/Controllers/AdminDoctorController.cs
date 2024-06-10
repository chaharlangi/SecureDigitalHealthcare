using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SecureDigitalHealthcare.Constants;
using SecureDigitalHealthcare.DTOs;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;

namespace SecureDigitalHealthcare.Controllers
{
    [Authorize(Policy = PolicyConstants.MustBeAdmin)]
    public class AdminDoctorController : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminDoctorController(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: AdminDoctor
        public async Task<IActionResult> Index()
        {
            var easyHealthContext = _context.Doctors.Include(d => d.IdNavigation).Include(d => d.Speciality);
            return View(await easyHealthContext.ToListAsync());
        }

        // GET: AdminDoctor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.IdNavigation)
                .Include(d => d.Speciality)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: AdminDoctor/Create
        public IActionResult Create()
        {
            ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Name");
            return View();
        }

        // POST: AdminDoctor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor, IFormFile profilePictureInput = null)
        {

            if (_context.Users.FirstOrDefault(x => x.Email == doctor.IdNavigation.Email) is not null)
            {
                AppDebug.Log("Email already exists");
                ViewData[ViewDataConstants.ValidationMessage] = Messages.EmailAlreadyExists;
                ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Name");
                return View();
            }

            doctor.IdNavigation.RegistrationDate = DateTime.UtcNow;

            doctor.IdNavigation.ProfileImagePath = AccountController.HandleNewProfilePicture(_environment, profilePictureInput);

            doctor.IdNavigation.RoleId = AppRole.GetRoleId(AppRole.Doctor);
            doctor.IdNavigation.RegistrationDate = DateTime.UtcNow;
            doctor.IdNavigation.Password = AppHasher.HashPassword(doctor.IdNavigation.Password!);

            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: AdminDoctor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Doctor doctor = await _context.Doctors.Include(x => x.IdNavigation).Include(x => x.Speciality).FirstOrDefaultAsync(x => x.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", doctor.Id);
            ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Name", doctor.SpecialityId);

            AdminEditDoctorDTO adminEditDoctorDTO = new AdminEditDoctorDTO
            {
                Id = doctor.Id,
                Name = doctor.IdNavigation.Name,
                LastName = doctor.IdNavigation.LastName,
                Email = doctor.IdNavigation.Email,
                SpecialityId = doctor.Speciality.Id,
                PhoneNumber = doctor.IdNavigation.PhoneNumber,
                Address = doctor.IdNavigation.Address,
                ProfileImagePath = doctor.IdNavigation.ProfileImagePath,
                Birthdate = doctor.IdNavigation.BirthDate
            };
            return View(adminEditDoctorDTO);
        }

        // POST: AdminDoctor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        private const string BindEditProfile = "Id,SpecialityId";
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, /*[Bind(BindEditProfile)]*/ AdminEditDoctorDTO doctor, IFormFile profilePictureInput = null)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (_context.Users.FirstOrDefault(x => x.Email == doctor.Email) is not null)
            {
                ViewData[ViewDataConstants.ValidationMessage] = Messages.EmailAlreadyExists;
                ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", doctor.Id);
                ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Name", doctor.SpecialityId);
                return View(doctor);
            }

            var user = await _context.Doctors.Include(x => x.IdNavigation).Include(x => x.Speciality).FirstOrDefaultAsync(x => x.Id == id);

            user.IdNavigation.Name = doctor.Name;
            user.IdNavigation.LastName = doctor.LastName;
            user.IdNavigation.Email = doctor.Email;
            user.IdNavigation.PhoneNumber = doctor.PhoneNumber;
            user.IdNavigation.Address = doctor.Address;
            user.IdNavigation.BirthDate = doctor.Birthdate;
            user.Speciality = await _context.Specialities.FirstOrDefaultAsync(x => x.Id == doctor.SpecialityId);
            //

            string newProfilePictureName = user.IdNavigation.ProfileImagePath;
            if (profilePictureInput != null)
            {
                AccountController.HandlePreviousProfilePicture(_environment, user.IdNavigation.ProfileImagePath);
                newProfilePictureName = AccountController.HandleNewProfilePicture(_environment, profilePictureInput);
            }
            user.IdNavigation.ProfileImagePath = newProfilePictureName;
            doctor.ProfileImagePath = newProfilePictureName;
            //
            //return Content(MyHelper.GetErrorListFromModelStateString(ModelState));

            await _context.SaveChangesAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", doctor.Id);
            ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Name", doctor.SpecialityId);

            return RedirectToAction(nameof(Details), new { id = id });
            //return RedirectToAction(nameof(Edit),id);
        }

        // GET: AdminDoctor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.IdNavigation)
                .Include(d => d.Speciality)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: AdminDoctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.Include(x => x.Doctor).FirstOrDefaultAsync(x => x.Id == id);

            if (user != null)
            {
                _context.Doctors.Remove(user.Doctor);
                _context.Users.Remove(user);
            }


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}
