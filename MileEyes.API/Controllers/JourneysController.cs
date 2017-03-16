using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.API.Models.GeocodingModels;
using MileEyes.API.Services;
using MileEyes.API.Extensions;
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

        [Route("api/JourneyCost/{journeyId}/")]
        public async Task<IHttpActionResult> GetJourneyCost(string journeyId)
        {
            var costs = new CostModel();

            var journey = db.Journeys.Find(Guid.Parse(journeyId));

            costs = journey.CalculateCost();

            journey.Cost = costs.Cost;
            journey.UnderDistance = costs.UnderDistance;
            journey.OverDistance = costs.OverDistance;

            await db.SaveChangesAsync();

            return Ok();
        }

        // GET: api/Journeys
        [Route("api/Journeys/{requestedDay}/{requestedMonth}")]
        public IQueryable<JourneyViewModel> GetJourneys(int requestedDay, int requestedMonth)
        {
            try { 
                var user = db.Users.Find(User.Identity.GetUserId());

                var drivers = user.Profiles.OfType<Driver>();

                var result = new List<JourneyViewModel>();
			    var requestedWeek = new DateTime(DateTime.Today.Year, requestedMonth, requestedDay);
			    var weekStart = requestedWeek;
			    var weekEnd = requestedWeek.AddDays(7);
                foreach (var driver in drivers)
                {
                    if (user.Email == driver.User.Email)
                    {
					    foreach (var j in driver.Journeys.OrderBy(dt => dt.Date))
					    {
						    if (j.Date == weekStart || j.Date > weekStart && j.Date < weekEnd || j.Date == weekEnd)
						    {
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
								    Vehicle = vehicle,
                                    Waypoints = j.Waypoints.Count
							    };
							    result.Add(journey);
						    }
					    }
                    }
                }
                return result.AsQueryable();
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception in Journey Controller: " + e);
                return null;
            }
        }

        // GET: api/Waypoints
        [Route("api/Waypoints/{journeyId}/{startingStep}")]
        public WaypointsViewModel GetWaypoints(string journeyId, int startingStep)
        {
            try
            {
                var journey = db.Journeys.Find(Guid.Parse(journeyId));
                
                var result = new WaypointsViewModel();
                result.Waypoints = new List<WaypointViewModel>();

                var newWaypoints = new WaypointsViewModel();

                var waypointsToProcess = journey.Waypoints.OrderBy(s => s.Step);
                
                for (int i = startingStep; startingStep + 50 > i; i++)
                {
                    if (i > waypointsToProcess.Count() - 1) break;
                    var waypoint = waypointsToProcess.ElementAt(i);
                    var newWaypoint = new WaypointViewModel()
                    {
                        Id = waypoint.Id.ToString(),
                        Latitude = waypoint.Address.Coordinates.Latitude,
                        Longitude = waypoint.Address.Coordinates.Longitude,
                        PlaceId = waypoint.Address.PlaceId,
                        Step = waypoint.Step,
                        Timestamp = waypoint.Timestamp
                    };

                    result.Waypoints.Add(newWaypoint);                        
                }
                result.journeyId = journey.Id.ToString();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception in Journey Controller: " + e);
                return null;
            }
        }
               
        [Route("api/Journeys/")]
        [ResponseType(typeof(JourneyViewModel))]
        public async Task<IHttpActionResult> PostJourney(JourneyBindingModel model)
        {
            try
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

                if (driver == null)
                {
                    return BadRequest();
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
                    Passengers = model.Passengers,
                    Modified = DateTimeOffset.UtcNow
                };
                
                // Calculate Cost and VAT
                //j.Cost = j.CalculateCost();
                //j.FuelVat = j.CalculateFuelVat();

                //j.Cost = Math.Round(j.Cost, 2);

                // Add Journey to the Database
                j = db.Journeys.Add(j);

//                try
//                {
                    // Save Changes
                 await db.SaveChangesAsync();
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex);
//                }

                // Return Journey with new Id
                return Ok(new JourneyViewModel()
                {
                    Accepted = j.Accepted,
                    Company = new CompanyViewModel()
                    {
                        Id = j.Company.Id.ToString()
                    },
                    //Cost = Convert.ToDouble(j.Cost),
                    Date = j.Date,
                    Distance = j.Distance,
                    Id = j.Id.ToString(),
                    Invoiced = j.Invoiced,
                    Passengers = j.Passengers,
                    Reason = j.Reason,
                    Rejected = j.Rejected                    
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception in Journey Controller: " + e);
                return null;
            }
        }

        [Route("api/Waypoints/")]
        [ResponseType(typeof(WaypointsViewModel))]
        public async Task<IHttpActionResult> PostWaypoints(WaypointsBindingModel model)
        {
            IList<Waypoint> waypoints = new List<Waypoint>();

            // Get User
            var user = db.Users.Find(User.Identity.GetUserId());

            var journey = db.Journeys.Find(Guid.Parse(model.journeyId));

            // Get Company
            var company = db.Companies.Find(journey.Company.Id);

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

            if (driver == null)
            {
                return BadRequest();
            }
            // Get Vehicle
            var vehicle = db.Vehicles.Find(journey.Vehicle.Id);

            if (vehicle == null) return BadRequest();
            
            if (model.Waypoints.Count == 2)
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

                newOrigin.Journey = journey;
                newDestination.Journey = journey;
                
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

                db.Waypoints.Add(newOrigin);
                db.Waypoints.Add(newDestination);
            }
            else
            {

                bool containedFirst = false;
                bool containedLast = false;
                var addressResult = new Models.GeocodingModels.Address();

                if (model.first == model.Waypoints.First().Step)
                {
                    var firstWaypoint = model.Waypoints.First();

                    addressResult = await GeocodingService.GetAddress(firstWaypoint.Latitude, firstWaypoint.Longitude);

                    var firstWp = new Waypoint();
                    firstWp.Id = Guid.NewGuid();
                    firstWp.Step = firstWaypoint.Step;
                    firstWp.Timestamp = firstWaypoint.Timestamp;
                    firstWp.Journey = journey;
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

                    waypoints.Add(firstWp);
                    containedFirst = true;
                }
                
                int startPoint = 0;

                if (containedFirst) startPoint = 1;

                int endPoint = model.Waypoints.Count;

                if (model.Waypoints.Last().Step == model.last)
                {
                    endPoint = model.Waypoints.Count - 1;
                    containedLast = true;
                }

                for (int i = startPoint; i < endPoint; i++)
                {
                    var currentWaypoint = model.Waypoints.ElementAt(i);
                    var newWaypoint = new Waypoint();
                    newWaypoint.Id = Guid.NewGuid();
                    newWaypoint.Journey = journey;
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
                    waypoints.Add(newWaypoint);
                }

                if (containedLast)
                {
                    var lastWaypoint = model.Waypoints.Last();

                    addressResult = await GeocodingService.GetAddress(lastWaypoint.Latitude, lastWaypoint.Longitude);

                    var lastWp = new Waypoint();
                    lastWp.Id = Guid.NewGuid();
                    lastWp.Step = lastWaypoint.Step;
                    lastWp.Timestamp = lastWaypoint.Timestamp;
                    lastWp.Journey = journey;
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

                    waypoints.Add(lastWp);
                }

                for (int i = 0; i < model.Waypoints.Count; i++)
                {
                    var waypoint = waypoints.ElementAt(i);
                    db.Waypoints.Add(waypoint);
                }
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        //TODO Redo the catching and use the database to store address for a month

        private static string GOOGLE_API_KEY = "AIzaSyArLAcqpQ1v_IxC_o0Qo41SYPUlGxKtMtI";

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