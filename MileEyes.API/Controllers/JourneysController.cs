using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using MileEyes.API.Extensions;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.API.Models.GeocodingModels;
using MileEyes.API.Services;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Journey;
using MileEyes.PublicModels.Vehicles;
using Newtonsoft.Json;
using Address = MileEyes.API.Models.DatabaseModels.Address;
using Coordinates = MileEyes.API.Models.DatabaseModels.Coordinates;

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class JourneysController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Journeys
        [Route("api/Journeys/{requestedDay}/{requestedMonth}")]
        public IQueryable<JourneyViewModel> GetJourneys(int requestedDay, int requestedMonth)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            var result = new List<JourneyViewModel>();

            var requestedWeek = new DateTime(DateTime.Today.Year, requestedMonth, requestedDay);
            var weekStart = requestedWeek;
            var weekEnd = requestedWeek.AddDays(7);

            try
            {
                foreach (var j in driver.Journeys.OrderBy(dt => dt.Date))
                {
                    if (j.Date > weekStart && j.Date < weekEnd)
                    {
                        var waypoints = j.Waypoints.Select(w => new WaypointViewModel()
                        {
                            Latitude = w.Address.Coordinates.Latitude,
                            Longitude = w.Address.Coordinates.Longitude,
                            PlaceId = w.Address.PlaceId,
                            Step = w.Step,
                            Timestamp = w.Timestamp,
                            Id = w.Id.ToString()
                        }).ToList();

                        var company = new CompanyViewModel()
                        {
                            Id = j.Company.Id.ToString()
                        };

                        var vehicle = new VehicleViewModel()
                        {
                            Id = j.Vehicle.Id.ToString()
                        };

                        var journey = new JourneyViewModel()
                        {
                            Accepted = j.Accepted,
                            Cost = Convert.ToDouble(j.Cost),
                            Date = j.Date,
                            Distance = j.Distance,
                            Id = j.Id.ToString(),
                            Invoiced = j.Invoiced,
                            Passengers = j.Passengers,
                            Reason = j.Reason,
                            Rejected = j.Rejected,
                            Company = company,
                            Waypoints = waypoints,
                            Vehicle = vehicle
                        };
                        result.Add(journey);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Caught exception in Journey Controller: " + e);
            }

            return result.AsQueryable();
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

            try
            {
                if (j.Driver.Id != driver.Id) return NotFound();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Caught Exception in Journey Controllor: " + e);
            }
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

            // Get User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get Company
            var company = db.Companies.Find(Guid.Parse(model.Company.CloudId));

            if (company == null) return BadRequest();

            var drivers = company.Profiles.OfType<Driver>();

            Driver driver = null;
            foreach (var d in drivers)
            {
                if (user.Email == d.User.Email)
                {
                    driver = d;
                    break;
                }
            }

            // Get Vehicle
            var vehicle = db.Vehicles.Find(Guid.Parse(model.Vehicle.CloudId));

            if (vehicle == null) return BadRequest();

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

//            // Loop through Waypoints in Model
//            foreach (var w in model.Waypoints)
//            {
//                // Create new Waypoint
//                var newWaypoint = new Waypoint()
//                {
//                    Id = Guid.NewGuid(),
//                    Step = w.Step,
//                    Timestamp = w.Timestamp
//                };
//
//                // Check if Model Waypoint has a PlaceId
//                if (!string.IsNullOrEmpty(w.PlaceId))
//                {
//                    // Get existing Addresses with PlaceId
//                    var existingAddresses = db.Addresses.Where(a => a.PlaceId == w.PlaceId);
//
//                    //Check if weve already stored the Address
//                    if (existingAddresses.Any())
//                    {
//                        newWaypoint.Address = existingAddresses.FirstOrDefault();
//                    }
//                    // Deal with us not having Address stored
//                    else
//                    {
//                        var addressResult = await GeocodingService.GetAddress(w.PlaceId);
//
//                        // Create a new Address
//                        newWaypoint.Address = new Address()
//                        {
//                            Id = Guid.NewGuid(),
//                            PlaceId = addressResult.PlaceId,
//                            Coordinates = new Coordinates()
//                            {
//                                Id = Guid.NewGuid(),
//                                Latitude = addressResult.Latitude,
//                                Longitude = addressResult.Longitude
//                            }
//                        };
//                    }
//                }
//                // Deal with having no PlaceId
//                else
//                {
//                    var existingAddresses =
//                        db.Addresses.Where(
//                            a =>
//                                Math.Abs(a.Coordinates.Latitude - w.Latitude) < 0.0005 &&
//                                Math.Abs(a.Coordinates.Longitude - w.Longitude) < 0.0005);
//                    //Check if weve already stored the Address
//                    if (existingAddresses.Any())
//                    {
//                        newWaypoint.Address = existingAddresses.FirstOrDefault();
//                    }
//                    // Deal with us not having Address stored
//                    else
//                    {
//                        var addressResult = await GeocodingService.GetAddress(w.Latitude, w.Longitude);
//                        if (addressResult == null) break;
//
//                        // Create a new Address
//                        newWaypoint.Address = new Address()
//                        {
//                            Id = Guid.NewGuid(),
//                            PlaceId = addressResult.PlaceId,
//                            Coordinates = new Coordinates()
//                            {
//                                Id = Guid.NewGuid(),
//                                Latitude = addressResult.Latitude,
//                                Longitude = addressResult.Longitude
//                            }
//                        };
//                    }
//                }
//                bool skipWaypoint = false;
//                if (j.Waypoints.Count < 1) skipWaypoint = false;
//                for (int i = 0; i < j.Waypoints.Count; i++)
//                {
//                    var waypoint = j.Waypoints.ElementAt(i);
//                    if (newWaypoint.Address.Coordinates.Longitude == waypoint.Address.Coordinates.Longitude &&
//                        newWaypoint.Address.Coordinates.Latitude == waypoint.Address.Coordinates.Latitude)
//                        skipWaypoint = true;
//                }
//                if (skipWaypoint) continue;
//                j.Waypoints.Add(newWaypoint);
//            }

            // If this is a GPS Journey, execute this code
            if (model.Waypoints.Count > 2)
            {
                var firstWaypoint = model.Waypoints.First();
                var lastWaypoint = model.Waypoints.Last();

                var addressResult = await GeocodingService.GetAddress(firstWaypoint.Latitude, firstWaypoint.Longitude);

                // Create a new Address
                var firstWp = new Waypoint();
                firstWp.Id = Guid.NewGuid();
                firstWp.Step = firstWaypoint.Step;
                firstWp.Timestamp = firstWaypoint.Timestamp;
                firstWp.Journey = j;
                firstWp.Address = new Address()
                {
                    Id = Guid.NewGuid(),
                    PlaceId = addressResult.PlaceId,
                    Coordinates = new Coordinates()
                    {
                        Id = Guid.NewGuid(),
                        Latitude = firstWaypoint.Latitude,
                        Longitude = firstWaypoint.Longitude
                    }
                };

                j.Waypoints.Add(firstWp);

                for (int i = 1; i < model.Waypoints.Count - 1; i++)
                {
                    var currentWaypoint = model.Waypoints.ElementAt(i);
                    var newWaypoint = new Waypoint();
                    newWaypoint.Id = Guid.NewGuid();

                    newWaypoint.Address = new Address()
                    {
                        Id = Guid.NewGuid(),
                        PlaceId = "     ",
                        Coordinates = new Coordinates()
                        {
                            Id = Guid.NewGuid(),
                            Latitude = currentWaypoint.Latitude,
                            Longitude = currentWaypoint.Longitude
                        }
                    };
                    newWaypoint.Step = currentWaypoint.Step;
                    newWaypoint.Timestamp = currentWaypoint.Timestamp;
                    newWaypoint.Journey = j;
                    j.Waypoints.Add(newWaypoint);
                }


                addressResult = await GeocodingService.GetAddress(lastWaypoint.Latitude, lastWaypoint.Longitude);

                // Create a new Address
                var lastWp = new Waypoint();
                lastWp.Id = Guid.NewGuid();
                lastWp.Step = lastWaypoint.Step;
                lastWp.Timestamp = lastWaypoint.Timestamp;
                lastWp.Journey = j;
                lastWp.Address = new Address()
                {
                    Id = Guid.NewGuid(),
                    PlaceId = addressResult.PlaceId,
                    Coordinates = new Coordinates()
                    {
                        Id = Guid.NewGuid(),
                        Latitude = lastWaypoint.Latitude,
                        Longitude = lastWaypoint.Longitude
                    }
                };

                j.Waypoints.Add(lastWp);
            }
            // Else, use a cut down version for only 2 Waypoints
            else
            {
                var newOrigin = new Waypoint();
                var newDestination = new Waypoint();
                var origin = model.Waypoints[0];
                var destination = model.Waypoints[1];

                newOrigin.Id = Guid.NewGuid();
                newDestination.Id = Guid.NewGuid();

                newOrigin.Step = origin.Step;
                newDestination.Step = destination.Step;

                newOrigin.Timestamp = origin.Timestamp;
                newDestination.Timestamp = destination.Timestamp;

                newOrigin.Journey = j;
                newDestination.Journey = j;

                newOrigin.Address = new Address()
                {
                    Id = Guid.NewGuid(),
                    PlaceId = origin.PlaceId,
                    Coordinates = new Coordinates()
                    {
                        Id = Guid.NewGuid(),
                        Latitude = origin.Latitude,
                        Longitude = origin.Longitude
                    }
                };
                newDestination.Address = new Address()
                {
                    Id = Guid.NewGuid(),
                    PlaceId = destination.PlaceId,
                    Coordinates = new Coordinates()
                    {
                        Id = Guid.NewGuid(),
                        Latitude = destination.Latitude,
                        Longitude = destination.Longitude
                    }
                };

                j.Waypoints.Add(newOrigin);
                j.Waypoints.Add(newDestination);
            }

            // Calculate Cost and VAT
            j.Cost = j.CalculateCost();
            j.FuelVat = j.CalculateFuelVat();

            // Add Journey to the Database
            j = db.Journeys.Add(j);

            try
            {
                // Save Changes
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

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

        private static String GOOGLE_API_KEY = "AIzaSyArLAcqpQ1v_IxC_o0Qo41SYPUlGxKtMtI";

        private static ArrayList PLACE_ID_CACHE = new ArrayList();

        private static ArrayList RESULT_CACHE = new ArrayList();

        private static ArrayList TIME_CACHE = new ArrayList();

        private static long TIMEOUT = 2592000000;

        [HttpGet]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("api/Journeys/Address")]
        public async Task<IHttpActionResult> GetAddress(String placeId)
        {
            try
            {
                var index = -1;
                if (PLACE_ID_CACHE.Contains(placeId))
                {
                    index = PLACE_ID_CACHE.IndexOf(placeId);
                    return Ok(JsonConvert.DeserializeObject<GeocodeResult>((String) RESULT_CACHE[index]));
                }

                var url = "https://maps.googleapis.com/maps/api/place/details/json?placeid=" + placeId + "&key=" +
                          GOOGLE_API_KEY;

                var response = await Helpers.HttpHelper.FileGetContents(url);

                if (string.IsNullOrEmpty(response))
                {
                    return BadRequest("Unknown address");
                }
                var result = JsonConvert.DeserializeObject<GeocodeResult>(response);
                if (result.status == "OVER_QUERY_LIMIT")
                {
                    System.Threading.Thread.Sleep(300);
                    response = await Helpers.HttpHelper.FileGetContents(url);

                    if (string.IsNullOrEmpty(response))
                    {
                        return BadRequest("Unknown address");
                    }
                    result = JsonConvert.DeserializeObject<GeocodeResult>(response);
                    if (result.status == "OVER_QUERY_LIMIT")
                    {
                        return BadRequest("Request limit!");
                    }
                }
                PLACE_ID_CACHE.Add(placeId);
                RESULT_CACHE.Add(result);
                long currentTime = new DateTime().Ticks;
                TIME_CACHE.Add(currentTime);
                ArrayList toRemove = new ArrayList();
                foreach (var id in PLACE_ID_CACHE)
                {
                    index = PLACE_ID_CACHE.IndexOf(id);
                    if (currentTime - TIMEOUT > (long) TIME_CACHE[index])
                    {
                        toRemove.Add(index);
                    }
                }
                foreach (var i in toRemove)
                {
                    PLACE_ID_CACHE.Remove((int) i);
                    RESULT_CACHE.Remove((int) i);
                    TIME_CACHE.Remove((int) i);
                }
                return Ok(result);
            }
            catch (NullReferenceException e)
            {
                //return BadRequest(e.ToString());
            }
            return BadRequest("Error trying to find address, please try again later.");
        }
    }
}