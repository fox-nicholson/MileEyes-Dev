using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Controllers
{
    public class FuelTypesManagerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FuelTypesManager
        public async Task<ActionResult> Index()
        {
            return View(await db.FuelTypes.ToListAsync());
        }

        // GET: FuelTypesManager/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FuelType fuelType = await db.FuelTypes.FindAsync(id);
            if (fuelType == null)
            {
                return HttpNotFound();
            }
            return View(fuelType);
        }

        // GET: FuelTypesManager/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FuelTypesManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] FuelType fuelType)
        {
            if (ModelState.IsValid)
            {
                fuelType.Id = Guid.NewGuid();
                db.FuelTypes.Add(fuelType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(fuelType);
        }

        // GET: FuelTypesManager/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FuelType fuelType = await db.FuelTypes.FindAsync(id);
            if (fuelType == null)
            {
                return HttpNotFound();
            }
            return View(fuelType);
        }

        // POST: FuelTypesManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] FuelType fuelType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fuelType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(fuelType);
        }

        // GET: FuelTypesManager/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FuelType fuelType = await db.FuelTypes.FindAsync(id);
            if (fuelType == null)
            {
                return HttpNotFound();
            }
            return View(fuelType);
        }

        // POST: FuelTypesManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            FuelType fuelType = await db.FuelTypes.FindAsync(id);
            db.FuelTypes.Remove(fuelType);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}