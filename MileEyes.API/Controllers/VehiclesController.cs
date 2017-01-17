using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.PublicModels.EngineTypes;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class VehiclesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Vehicles
        public IQueryable<VehicleViewModel> GetVehicles()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            if (driver != null)
            {
                return driver.Vehicles.Select(v => new VehicleViewModel()
                {
                    Id = v.Id.ToString(),
                    Registration = v.Registration,
                    EngineType = new EngineTypeViewModel()
                    {
                        Id = v.EngineType.Id.ToString()
                    }
                }).AsQueryable();
            }

            return new List<VehicleViewModel>().AsQueryable();
        }

        // GET: api/Vehicles/5
        [ResponseType(typeof(VehicleViewModel))]
        public async Task<IHttpActionResult> GetVehicle(Guid id)
        {
            Vehicle vehicle = await db.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(new VehicleViewModel()
            {
                Id = vehicle.Id.ToString(),
                Registration = vehicle.Registration,
                EngineType = new EngineTypeViewModel()
                {
                    Id = vehicle.EngineType.Id.ToString()
                }
            });
        }

        // POST: api/Vehicles
        [ResponseType(typeof(VehicleViewModel))]
        public async Task<IHttpActionResult> PostVehicle(VehicleBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            var engineTypeGuid = Guid.Parse(model.EngineType.Id);

            var engineType = db.EngineTypes.FirstOrDefault(et => et.Id == engineTypeGuid);

            if (engineType == null)
            {
                return BadRequest();
            }

            var stripedRegistration = model.Registration.Trim(' ').ToUpper();

            Vehicle vehicle;

            var existingVehicles = db.Vehicles.Where(v => v.Registration == stripedRegistration);

            if (existingVehicles.Any())
            {
                vehicle = existingVehicles.FirstOrDefault();
            }
            else
            {
                vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Registration = model.Registration,
                    EngineType = engineType
                };
            }

            driver.Vehicles.Add(vehicle);

            db.Vehicles.Add(vehicle);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehicleExists(vehicle.Id))
                {
                    return Conflict();
                }

                throw;
            }

            return CreatedAtRoute("DefaultApi", new { id = vehicle.Id }, new VehicleViewModel()
            {
                Id = vehicle.Id.ToString(),
                Registration = vehicle.Registration,
                EngineType = new EngineTypeViewModel()
                {
                    Id = vehicle.EngineType.Id.ToString()
                }
            });
        }

        // DELETE: api/Vehicles/5
        [ResponseType(typeof(Vehicle))]
        public async Task<IHttpActionResult> DeleteVehicle(Guid id)
        {
            Vehicle vehicle = await db.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            driver.Vehicles.Remove(vehicle);

            await db.SaveChangesAsync();

            return Ok(new VehicleViewModel()
            {
                Id = vehicle.Id.ToString(),
                Registration = vehicle.Registration,
                EngineType = new EngineTypeViewModel()
                {
                    Id = vehicle.EngineType.Id.ToString()
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VehicleExists(Guid id)
        {
            return db.Vehicles.Count(e => e.Id == id) > 0;
        }
    }
}