using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.API.Services;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Driver;
using MileEyes.PublicModels.Journey;
using MileEyes.PublicModels.Vehicles;
using MileEyes.PublicModels.EngineTypes;

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class CompaniesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Companies
        public IQueryable<CompanyViewModel> GetCompanies()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var result = new List<CompanyViewModel>();
            try
            {
                var drivers = user.Profiles.OfType<Driver>();
                var accountants = user.Profiles.OfType<Accountant>();
                var managers = user.Profiles.OfType<Manager>();
                var owners = user.Profiles.OfType<Owner>();


                var companies = new List<Company>();

                foreach (var d in drivers)
                {
                    foreach (var c in d.Companies)
                    {
                        if (companies.Contains(c))
                        {
                            continue;
                        }
                        companies.Add(c);
                    }
                }
                foreach (var a in accountants)
                {
                    foreach (var c in a.Companies)
                    {
                        if (companies.Contains(c))
                        {
                            continue;
                        }
                        companies.Add(c);
                    }
                }
                foreach (var m in managers)
                {
                    foreach (var c in m.Companies)
                    {
                        if (companies.Contains(c))
                        {
                            continue;
                        }
                        companies.Add(c);
                    }
                }
                foreach (var o in owners)
                {
                    foreach (var c in o.Companies)
                    {
                        if (companies.Contains(c))
                        {
                            continue;
                        }
                        companies.Add(c);
                    }
                }
                foreach (var c in companies)
                {
                    result.Add(new CompanyViewModel()
                    {
                        Id = c.Id.ToString(),
                        Name = c.Name,
                        Personal = c.Personal,
                        LowRate = c.LowRate,
                        HighRate = c.HighRate
                    });
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            return result.AsQueryable();
        }

        [ResponseType(typeof(CompanyViewModel))]
        public async Task<IHttpActionResult> PostCompany(CompanyBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());


            // Get existing addresses with the same PlaceId
            var existingAddress = db.Addresses.FirstOrDefault(a => a.PlaceId == model.PlaceId);

            // Checks if the posted company name has already been taken.
            var existingCompany = db.Companies.FirstOrDefault(c => c.Name == model.Name);
            if (existingCompany != null)
            {
                return BadRequest("Company name already taken!");
            }

            Address address;

            // Is there any addresses?
            if (existingAddress == null)
            {
                // No so we need to check with Google and get the details
                var addressResult = await GeocodingService.GetAddress(model.PlaceId);

                // Create the new Address in database
                address = new Address()
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
            else
            {
                // Theres an existing adress stored so use that one
                address = existingAddress;
            }

            var owner = new Owner
            {
                Id = Guid.NewGuid()
            };
            var accountant = new Accountant
            {
                Id = Guid.NewGuid()
            };
            var manager = new Manager
            {
                Id = Guid.NewGuid()
            };
            var driver = new Driver
            {
                Id = Guid.NewGuid()
            };

            // Setup the new company
            var newCompany = new Company()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                AutoAccept = model.AutoAccept,
                HighRate = model.HighRate,
                LowRate = model.LowRate,
                Vat = model.Vat,
                VatNumber = model.VatNumber,
                Address = address,
                AutoAcceptDistance = model.AutoAcceptDistance,
                Owner = owner,
                Personal = false
            };

            newCompany.Profiles.Add(owner);
            newCompany.Profiles.Add(driver);
            newCompany.Profiles.Add(manager);
            newCompany.Profiles.Add(accountant);

            db.Companies.Add(newCompany);

            // Save the changes
            await db.SaveChangesAsync();

            // Return the new company's details
            return Ok(new CompanyViewModel()
            {
                Id = newCompany.Id.ToString(),
                Name = model.Name,
                HighRate = model.HighRate,
                LowRate = model.LowRate,
                Personal = false
            });
        }

        [Route("api/Companies/{companyId}/Journeys")]
        public IQueryable<JourneyViewModel> GetJourneys(Guid companyId)
        {
            var result = new List<JourneyViewModel>();

            var user = db.Users.Find(User.Identity.GetUserId());

            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var accountant = user.Profiles.OfType<Accountant>().FirstOrDefault();

            try
            {
                var company = db.Companies.Find(companyId);

                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();
                var companyAccountants = company.Profiles.OfType<Accountant>();

                if (companyOwners.Contains(owner) || companyManagers.Contains(manager) ||
                    companyAccountants.Contains(accountant))
                {
                    var journeys = company.Journeys;

                    foreach (var j in journeys)
                    {
                        result.Add(new JourneyViewModel()
                        {
                            Accepted = j.Accepted,
                            Company = new CompanyViewModel()
                            {
                                Id = j.Company.Id.ToString()
                            },
                            Cost = Convert.ToDouble(j.Cost),
                            Date = j.Date,
                            Driver = new DriverViewModel()
                            {
                                Id = j.Driver.Id.ToString(),
                                FirstName = j.Driver.User.FirstName,
                                LastName = j.Driver.User.LastName,
                                LastActiveVehicle = j.Driver.LastActiveVehicle
                            },
                            Vehicle = new VehicleViewModel()
                            {
                                Id = j.Vehicle.Id.ToString(),
                                Registration = j.Vehicle.Registration,
                                EngineType = new EngineTypeViewModel()
                                {
                                    Id = j.Vehicle.EngineType.Id.ToString(),
                                    Name = j.Vehicle.EngineType.Name
                                }
                            },
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
                else
                {
                    var journeys = company.Journeys;

                    foreach (var j in journeys)
                    {
                        if (user.Email != j.Driver.User.Email)
                        {
                            continue;
                        }
                        result.Add(new JourneyViewModel()
                        {
                            Accepted = j.Accepted,
                            Company = new CompanyViewModel()
                            {
                                Id = j.Company.Id.ToString()
                            },
                            Cost = Convert.ToDouble(j.Cost),
                            Date = j.Date,
                            Driver = new DriverViewModel()
                            {
                                Id = j.Driver.Id.ToString(),
                                FirstName = j.Driver.User.FirstName,
                                LastName = j.Driver.User.LastName,
                                LastActiveVehicle = j.Driver.LastActiveVehicle
                            },
                            Vehicle = new VehicleViewModel()
                            {
                                Id = j.Vehicle.Id.ToString(),
                                Registration = j.Vehicle.Registration,
                                EngineType = new EngineTypeViewModel()
                                {
                                    Id = j.Vehicle.EngineType.Id.ToString(),
                                    Name = j.Vehicle.EngineType.Name
                                }
                            },
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
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            return result.AsQueryable();
        }

        [Route("api/Companies/{companyId}/Journeys/{journeyId}/Accept")]
        public async Task<IHttpActionResult> AcceptCompanyJourney(Guid companyId, Guid journeyId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();

                // Check if the current users has rights, if not respond with bad request
                if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

                var journey = db.Journeys.Find(journeyId);

                if (!company.Journeys.Contains(journey)) return BadRequest();

                try
                {
                    journey.Accepted = true;
                    journey.Rejected = false;
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Companies/{companyId}/Journeys/{journeyId}/Reject")]
        public async Task<IHttpActionResult> RejectCompanyJourney(Guid companyId, Guid journeyId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();

                // Check if the current users has rights, if not respond with bad request
                if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

                var journey = db.Journeys.Find(journeyId);

                if (!company.Journeys.Contains(journey)) return BadRequest();

                try
                {
                    journey.Accepted = false;
                    journey.Rejected = true;
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Companies/{companyId}/Journeys/{journeyId}/PushToAccountancy/{accountingPackage}")]
        public async Task<IHttpActionResult> PushCompanyJourneyToAccouncy(Guid companyId, Guid journeyId,
            string accountancyPackage)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();

                // Check if the current users has rights, if not respond with bad request
                if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }

            /*
             * Implement accounting push here using respective API's, Fox has links to the relevant docs for this
             */


            // Always returns Bad until implemented
            return BadRequest();
        }

        [HttpGet]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("api/Companies/{companyId}/Drivers")]
        public IQueryable<DriverViewModel> GetDrivers(Guid companyId)
        {
            var result = new List<DriverViewModel>();

            var user = db.Users.Find(User.Identity.GetUserId());

            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var accountant = user.Profiles.OfType<Accountant>().FirstOrDefault();

            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();
                var companyAccountants = company.Profiles.OfType<Accountant>();

                // Check if the current users has rights, if not respond with bad request
                if (companyOwners.Contains(owner) || companyManagers.Contains(manager) ||
                    companyAccountants.Contains(accountant))
                {
                    var drivers = company.Profiles.OfType<Driver>();
                    foreach (var d in drivers)
                    {
                        result.Add(new DriverViewModel()
                        {
                            Id = d.Id.ToString(),
                            LastActiveVehicle = d.LastActiveVehicle,
                            FirstName = d.User.FirstName,
                            LastName = d.User.LastName,
                            Email = d.User.Email,
                            Vehicles = d.Vehicles.Select(v => new VehicleViewModel()
                            {
                                Id = v.Id.ToString(),
                                Registration = v.Registration,
                                EngineType = new EngineTypeViewModel()
                                {
                                    Id = v.EngineType.Id.ToString(),
                                    Name = v.EngineType.Name
                                }
                            }).ToList()
                        });
                    }
                }
                else
                {
                    var drivers = company.Profiles.OfType<Driver>();
                    foreach (var d in drivers)
                    {
                        if (d.User.Email != user.Email)
                        {
                            continue;
                        }
                        result.Add(new DriverViewModel()
                        {
                            Id = d.Id.ToString(),
                            LastActiveVehicle = d.LastActiveVehicle,
                            FirstName = d.User.FirstName,
                            LastName = d.User.LastName,
                            Email = d.User.Email,
                            Vehicles = d.Vehicles.Select(v => new VehicleViewModel()
                            {
                                Id = v.Id.ToString(),
                                Registration = v.Registration,
                                EngineType = new EngineTypeViewModel()
                                {
                                    Id = v.EngineType.Id.ToString(),
                                    Name = v.EngineType.Name
                                }
                            }).ToList()
                        });
                    }
                }
            }
            catch (NullReferenceException e)
            {
                //return BadRequest(e.ToString());
            }
            return result.AsQueryable();
        }


        [HttpPost]
        [Route("api/Companies/{companyId}/AddDriver")]
        public async Task<IHttpActionResult> AddCompanyDriver(Guid companyId, DriverBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            try
            {
                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();

                // Check if the current users has rights, if not respond with bad request
                if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // get the profile of the driver to be added
            var newDriver = db.Profiles.Find(Guid.Parse(model.Id)) as Driver;

            // If the driver doesnt exist then return BadRequest
            if (newDriver == null) return BadRequest();

            try
            {
                // Add the driver to the company
                company.Profiles.Add(newDriver);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // Save the changes
            await db.SaveChangesAsync();

            // Return the driver so that client/browser can show success
            return Ok(new DriverViewModel()
            {
                Id = newDriver.Id.ToString()
            });
        }

        [HttpPost]
        [ResponseType(typeof(DriverViewModel))]
        [Route("api/Companies/{companyId}/RemoveDriver/{driverId}")]
        public async Task<IHttpActionResult> RemoveCompanyDriver(Guid companyId, Guid driverId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            try
            {
                // get the company owners and managers
                var companyOwners = company.Profiles.OfType<Owner>();
                var companyManagers = company.Profiles.OfType<Manager>();

                // Check if the current users has rights, if not respond with bad request
                if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // Get the driver were removing
            var driver = db.Drivers.Find(driverId);

            try
            {
                // If the driver isnt part of the company return bad request
                if (!company.Profiles.Contains(driver)) return BadRequest();

                // Remove the driver from the company
                company.Profiles.Remove(driver);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // Save changes
            await db.SaveChangesAsync();

            // Return the drivers Id to show complete
            try
            {
                return Ok(new DriverViewModel()
                {
                    Id = driver.Id.ToString()
                });
            }
            catch (NullReferenceException)
            {
                return null;
            }
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