using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;

namespace ETFTalentProgram.Controllers
{
    public class StudentProfilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentProfilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudentProfil
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StudentProfili.Include(s => s.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StudentProfil/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentProfil = await _context.StudentProfili
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studentProfil == null)
            {
                return NotFound();
            }

            return View(studentProfil);
        }

        // GET: StudentProfil/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id");
            return View();
        }

        // POST: StudentProfil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Rang,Biografija,Vjestine,Projekti,PreferiraneLokacije,PreferiraneTehnologije,DostupanOd,DatumAzuriranja,StatusVerifikacije,StudentId")] StudentProfil studentProfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studentProfil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", studentProfil.StudentId);
            return View(studentProfil);
        }

        // GET: StudentProfil/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentProfil = await _context.StudentProfili.FindAsync(id);
            if (studentProfil == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", studentProfil.StudentId);
            return View(studentProfil);
        }

        // POST: StudentProfil/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Rang,Biografija,Vjestine,Projekti,PreferiraneLokacije,PreferiraneTehnologije,DostupanOd,DatumAzuriranja,StatusVerifikacije,StudentId")] StudentProfil studentProfil)
        {
            if (id != studentProfil.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studentProfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentProfilExists(studentProfil.Id))
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
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", studentProfil.StudentId);
            return View(studentProfil);
        }

        // GET: StudentProfil/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentProfil = await _context.StudentProfili
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studentProfil == null)
            {
                return NotFound();
            }

            return View(studentProfil);
        }

        // POST: StudentProfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studentProfil = await _context.StudentProfili.FindAsync(id);
            if (studentProfil != null)
            {
                _context.StudentProfili.Remove(studentProfil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentProfilExists(long id)
        {
            return _context.StudentProfili.Any(e => e.Id == id);
        }
    }
}
