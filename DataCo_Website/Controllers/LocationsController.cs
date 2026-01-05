using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataCo_Website.Models;

namespace DataCo_Website.Controllers
{
    public class LocationsController : Controller
    {
        private readonly DataCoTestContext _context;

        public LocationsController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Locations.ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Zipcode == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Zipcode,City,Country,State")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Zipcode,City,Country,State")] Location location)
        {
            if (id != location.Zipcode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Zipcode))
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
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Zipcode == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                _context.Locations.Remove(location);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(string id)
        {
            return _context.Locations.Any(e => e.Zipcode == id);
        }
        // GET: /Locations/GetCountries
        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _context.Locations
                .Select(l => l.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            return Json(countries);
        }

        // GET: /Locations/GetStates?country=...
        [HttpGet]
        public async Task<IActionResult> GetStates(string country)
        {
            if (string.IsNullOrEmpty(country)) return Json(new List<string>());

            var states = await _context.Locations
                .Where(l => l.Country == country)
                .Select(l => l.State)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            return Json(states);
        }

        // GET: /Locations/GetCities?country=...&state=...
        [HttpGet]
        public async Task<IActionResult> GetCities(string country, string state)
        {
            if (string.IsNullOrEmpty(state)) return Json(new List<string>());

            var cities = await _context.Locations
                .Where(l => l.Country == country && l.State == state)
                .Select(l => l.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            return Json(cities);
        }

        // GET: /Locations/GetZipcodes?country=...&state=...&city=...
        [HttpGet]
        public async Task<IActionResult> GetZipcodes(string country, string state, string city)
        {
            if (string.IsNullOrEmpty(city)) return Json(new List<string>());

            var zips = await _context.Locations
                .Where(l => l.Country == country && l.State == state && l.City == city)
                .Select(l => l.Zipcode)
                .Distinct()
                .OrderBy(z => z)
                .ToListAsync();
            return Json(zips);
        }

    }
}
