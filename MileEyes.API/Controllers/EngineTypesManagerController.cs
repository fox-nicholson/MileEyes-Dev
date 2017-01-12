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
    public class EngineTypesManagerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EngineTypesManager
        public async Task<ActionResult> Index()
        {
            return View(await db.EngineTypes.ToListAsync());
        }

        // GET: EngineTypesManager/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EngineType engineType = await db.EngineTypes.FindAsync(id);
            if (engineType == null)
            {
                return HttpNotFound();
            }
            return View(engineType);
        }

        // GET: EngineTypesManager/Create
        public ActionResult Create()
        {
            ViewBag.AvailableFuelTypes = db.FuelTypes.Select(ft => new SelectListItem()
            {
                Value = ft.Id.ToString(),
                Text = ft.Name
            });

            return View();
        }

        // POST: EngineTypesManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Size,FuelRate,Name,FuelType")] EngineType engineType)
        {
            ViewBag.AvailableFuelTypes = db.FuelTypes.Select(ft => new SelectListItem()
            {
                Value = ft.Id.ToString(),
                Text = ft.Name
            });

            if (ModelState.IsValid)
            {
                engineType.Id = Guid.NewGuid();

                engineType.FuelType = db.FuelTypes.Find(engineType.FuelType.Id);

                db.EngineTypes.Add(engineType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(engineType);
        }

        // GET: EngineTypesManager/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EngineType engineType = await db.EngineTypes.FindAsync(id);
            if (engineType == null)
            {
                return HttpNotFound();
            }
            return View(engineType);
        }

        // POST: EngineTypesManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Size,FuelRate,Name")] EngineType engineType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(engineType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(engineType);
        }

        // GET: EngineTypesManager/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EngineType engineType = await db.EngineTypes.FindAsync(id);
            if (engineType == null)
            {
                return HttpNotFound();
            }
            return View(engineType);
        }

        // POST: EngineTypesManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            EngineType engineType = await db.EngineTypes.FindAsync(id);
            db.EngineTypes.Remove(engineType);
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
