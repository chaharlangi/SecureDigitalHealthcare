using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;

namespace SecureDigitalHealthcare.Controllers
{
    [Authorize(Roles = AppRole.Admin)]
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
            doctor.IdNavigation.RegistrationDate = DateTime.Now;

            doctor.IdNavigation.ProfileImagePath = AccountController.HandleNewProfilePicture(_environment, profilePictureInput);

            doctor.IdNavigation.RoleId = AppRole.GetRoleId(AppRole.Doctor);
            doctor.IdNavigation.RegistrationDate = DateTime.Now;
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

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", doctor.Id);
            ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Id", doctor.SpecialityId);
            return View(doctor);
        }

        // POST: AdminDoctor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SpecialityId")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

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
            ViewData["SpecialityId"] = new SelectList(_context.Specialities, "Id", "Id", doctor.SpecialityId);
            return View(doctor);
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
            //var doctor = await _context.Doctors.FindAsync(id);
            //if (doctor != null)
            //{
            //    _context.Doctors.Remove(doctor);
            //}
            var user = await _context.Users.Include(x => x.Doctor).FirstOrDefaultAsync(x => x.Id == id);
            //return Content($"User deleted: {user.Doctor.SpecialityId}");
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
