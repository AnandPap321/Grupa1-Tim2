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
    public class EmailNotifikacijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmailNotifikacijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmailNotifikacija
        public async Task<IActionResult> Index()
        {
            return View(await _context.EmailNotifikacije.ToListAsync());
        }

        // GET: EmailNotifikacija/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailNotifikacija = await _context.EmailNotifikacije
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailNotifikacija == null)
            {
                return NotFound();
            }

            return View(emailNotifikacija);
        }

        // GET: EmailNotifikacija/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmailNotifikacija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TipNotifikacije,Sadrzaj,EmailPrimaoca,DatumKreiranja,DatumSlanja,StatusIsporuke,BrojPokusaja")] EmailNotifikacija emailNotifikacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emailNotifikacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(emailNotifikacija);
        }

        // GET: EmailNotifikacija/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailNotifikacija = await _context.EmailNotifikacije.FindAsync(id);
            if (emailNotifikacija == null)
            {
                return NotFound();
            }
            return View(emailNotifikacija);
        }

        // POST: EmailNotifikacija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TipNotifikacije,Sadrzaj,EmailPrimaoca,DatumKreiranja,DatumSlanja,StatusIsporuke,BrojPokusaja")] EmailNotifikacija emailNotifikacija)
        {
            if (id != emailNotifikacija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emailNotifikacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailNotifikacijaExists(emailNotifikacija.Id))
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
            return View(emailNotifikacija);
        }

        // GET: EmailNotifikacija/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailNotifikacija = await _context.EmailNotifikacije
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailNotifikacija == null)
            {
                return NotFound();
            }

            return View(emailNotifikacija);
        }

        // POST: EmailNotifikacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var emailNotifikacija = await _context.EmailNotifikacije.FindAsync(id);
            if (emailNotifikacija != null)
            {
                _context.EmailNotifikacije.Remove(emailNotifikacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmailNotifikacijaExists(long id)
        {
            return _context.EmailNotifikacije.Any(e => e.Id == id);
        }
    }
}
