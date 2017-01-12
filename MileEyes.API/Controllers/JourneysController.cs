using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using MileEyes.API.Extensions;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.API.Services;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Journey;

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class JourneysController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: api/Journeys
        public IQueryable<JourneyViewModel> GetJourneys()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            return db.Journeys.Where(j => j.Driver.Id == driver.Id).Select(j => new JourneyViewModel()
            {
                Accepted = j.Accepted,
                Company = new CompanyViewModel()
                {
                    Id = j.Company.Id.ToString()
                },
                Cost = Convert.ToDouble(j.Cost),
                Date = j.Date,
                Distance = j.Distance,
                Id = j.Id.ToString(),
                Invoiced = j.Invoiced,
                Passengers = j.Passengers,
                Reason = j.Reason,
                Rejected = j.Rejected,
                Waypoints = j.Waypoints.Select(w => new WaypointViewModel()
                {
                    Latitude = w.Address.Coordinates.Latitude,
                    Longitude = w.Address.Coordinates.Longitude,
                    PlaceId = w.Address.PlaceId,
                    Step = w.Step,
                    Timestamp = w.Timestamp,
                    Id = w.Id.ToString()
                }).ToList()
            });
        }

        // GET: api/Journeys/5
        [ResponseType(typeof(JourneyViewModel))]
        public async Task<IHttpActionResult> GetJourney(Guid id)
        {
            Journey j = await db.Journeys.FindAsync(id);
            if (j == null)
            {
                return NotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            if (j.Driver.Id != driver.Id) return NotFound();

            return Ok(new JourneyViewModel()
            {
                Accepted = j.Accepted,
                Company = new CompanyViewModel()
                {
                    Id = j.Company.Id.ToString()
                },
                Cost = Convert.ToDouble(j.Cost),
                Date = j.Date,
                Distance = j.Distance,
                Id = j.Id.ToString(),
                Invoiced = j.Invoiced,
                Passengers = j.Passengers,
                Reason = j.Reason,
                Rejected = j.Rejected,
                Waypoints = j.Waypoints.Select(w => new WaypointViewModel()
                {
                    Latitude = w.Address.Coordinates.Latitude,
                    Longitude = w.Address.Coordinates.Longitude,
                    PlaceId = w.Address.PlaceId,
                    Step = w.Step,
                    Timestamp = w.Timestamp,
                    Id = w.Id.ToString()
                }).ToList()
            });
        }

        [ResponseType(typeof(JourneyViewModel))]
        public async Task<IHttpActionResult> PostJourney(JourneyBindingModel model)
        {
            // Check Model State
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get User and then Driver Profile
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            // Get Company
            var company = await db.Companies.FindAsync(model.Company.Id);

            if (company == null) return BadRequest();

            // Get Vehicle
            var vehicle = db.Vehicles.Find(model.Vehicle.Id);

            if(vehicle == null) return BadRequest();

            // Create new Journey
            var j = new Journey()
            {
                Id = Guid.NewGuid(),
                Driver = driver,
                Accepted = false,
                Rejected = false,
                Company = company,
                Vehicle = vehicle,
                Date = model.Date,
                Distance = model.Distance,
                Reason = model.Reason,
                Invoiced = model.Invoiced,
                Passengers = model.Passengers
            };

            // Loop through Waypoints in Model
            foreach (var w in model.Waypoints)
            {
                // Create new Waypoint
                var newWaypoint = new Waypoint()
                {
                    Id = Guid.NewGuid(),
                    Step = w.Step,
                    Timestamp = w.Timestamp
                };

                // Check if Model Waypoint has a PlaceId
                if (!string.IsNullOrEmpty(w.PlaceId))
                {
                    // Get existing Addresses with PlaceId
                    var existingAddresses = db.Addresses.Where(a => a.PlaceId == w.PlaceId);

                    //Check if weve already stored the Address
                    if (existingAddresses.Any())
                    {
                        newWaypoint.Address = existingAddresses.FirstOrDefault();
                    }
                    // Deal with us not having Address stored
                    else
                    {
                        var addressResult = await GeocodingService.GetAddress(w.PlaceId);

                        // Create a new Address
                        newWaypoint.Address = new Address()
                        {
                            Id = Guid.NewGuid(),
                            PlaceId = addressResult.PlaceId,
                            Coordinates = new Coordinates()
                            {
                                Id = Guid.NewGuid(),
                                Latitude = addressResult.Latitude,
                                Longitude = addressResult.Longitude
                            }
                        };
                    }
                }
                // Deal with having no PlaceId
                else
                {
                    var existingAddresses =
                        db.Addresses.Where(
                            a =>
                                Math.Abs(a.Coordinates.Latitude - w.Latitude) < 0.0005 &&
                                Math.Abs(a.Coordinates.Longitude - w.Longitude) < 0.0005);

                }
            }

            // Calculate Cost and VAT
            j.Cost = j.CalculateCost();
            j.FuelVat = j.CalculateFuelVat();

            // Add Journey to the Database
            j = db.Journeys.Add(j);

            // Save Changes
            await db.SaveChangesAsync();

            // Return Journey with new Id
            return Ok(new JourneyViewModel()
            {
                Accepted = j.Accepted,
                Company = new CompanyViewModel()
                {
                    Id = j.Company.Id.ToString()
                },
                Cost = Convert.ToDouble(j.Cost),
                Date = j.Date,
                Distance = j.Distance,
                Id = j.Id.ToString(),
                Invoiced = j.Invoiced,
                Passengers = j.Passengers,
                Reason = j.Reason,
                Rejected = j.Rejected,
                Waypoints = j.Waypoints.Select(w => new WaypointViewModel()
                {
                    Latitude = w.Address.Coordinates.Latitude,
                    Longitude = w.Address.Coordinates.Longitude,
                    PlaceId = w.Address.PlaceId,
                    Step = w.Step,
                    Timestamp = w.Timestamp,
                    Id = w.Id.ToString()
                }).ToList()
            });
        }
    }
}
