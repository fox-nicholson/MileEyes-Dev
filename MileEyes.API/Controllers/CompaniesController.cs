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
using MileEyes.PublicModels.VehicleTypes;
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
                var managers = user.Profiles.OfType<Manager>();
                var owners = user.Profiles.OfType<Owner>();

                var companies = new List<Company>();

                var companiesOwner = new List<Company>();
                var companiesManager = new List<Company>();
                var companiesDriver = new List<Company>();

                foreach (var d in drivers)
                {
                    foreach (var c in d.Companies)
                    {
                        if (!companies.Contains(c))
                        {
                            companies.Add(c);
                        }
                        companiesDriver.Add(c);
                    }
                }
                foreach (var m in managers)
                {
                    foreach (var c in m.Companies)
                    {
                        if (!companies.Contains(c))
                        {
                            companies.Add(c);
                        }
                        companiesManager.Add(c);
                    }
                }
                foreach (var o in owners)
                {
                    foreach (var c in o.Companies)
                    {
                        if (!companies.Contains(c))
                        {
                            companies.Add(c);
                        }
                        companiesOwner.Add(c);
                    }
                }

                foreach (var c in companies)
                {
                    var rights = "";
                    if (companiesOwner.Contains(c))
                    {
                        rights = "3";
                    }
                    if (companiesManager.Contains(c))
                    {
                        if (rights == "")
                        {
                            rights = "2";
                        }
                        else
                        {
                            rights += ",2";
                        }
                    }
                    if (companiesDriver.Contains(c))
                    {
                        if (rights == "")
                        {
                            rights = "1";
                        }
                        else
                        {
                            rights += ",1";
                        }
                    }
                    result.Add(new CompanyViewModel()
                    {
                        Id = c.Id.ToString(),
                        Name = c.Name,
                        LowRate = c.LowRate,
                        HighRate = c.HighRate,
                        Rights = rights
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            return result.AsQueryable();
        }

        // GET: api/DriverCompanies
        [Route("api/DriverCompanies")]
        public IQueryable<CompanyViewModel> GetDriverCompanies()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var result = new List<CompanyViewModel>();
            try
            {
                var drivers = user.Profiles.OfType<Driver>();

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
                        result.Add(new CompanyViewModel()
                        {
                            Id = c.Id.ToString(),
                            Name = c.Name,
                            LowRate = c.LowRate,
                            HighRate = c.HighRate
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            return result.AsQueryable();
        }

        [ResponseType(typeof(CompanyViewModel))]
        public async Task<IHttpActionResult> PostCompany(CompanyBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());
                        
            var owner = new Owner
            {
                Id = Guid.NewGuid(),
                User = user
            };
            var accountant = new Accountant
            {
                Id = Guid.NewGuid(),
                User = user
            };
            var manager = new Manager
            {
                Id = Guid.NewGuid(),
                User = user
            };
            var driver = new Driver
            {
                Id = Guid.NewGuid(),
                User = user
            };



            // Setup the new company
            var newCompany = new Company()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                HighRate = model.HighRate,
                LowRate = model.LowRate,
                Owner = owner,
            };

            user.Profiles.Add(owner);

            newCompany.Profiles.Add(owner);

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
            });
        }

        [Route("api/Companies/{companyId}/Journeys")]
        public IQueryable<JourneyViewModel> GetJourneys(Guid companyId)
        {
            var result = new List<JourneyViewModel>();

            var user = db.Users.Find(User.Identity.GetUserId());
            try
            {
                var company = db.Companies.Find(companyId);

                var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
				var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);

                if (owner != null || manager != null || accountant != null)
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
                            Modified = j.Modified,
                            Driver = new DriverViewModel()
                            {
                                Id = j.Driver.Id.ToString(),
                                FirstName = j.Driver.User.FirstName,
                                LastName = j.Driver.User.LastName,
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
                            Rejected = j.Rejected
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
                            Modified = j.Modified,
                            Driver = new DriverViewModel()
                            {
                                Id = j.Driver.Id.ToString(),
                                FirstName = j.Driver.User.FirstName,
                                LastName = j.Driver.User.LastName,
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
                            Rejected = j.Rejected
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
            
            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);

                // Check if the current users has rights, if not respond with bad request
                if (owner == null && manager == null) return BadRequest();

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

            try
            {
                // get the company
                var company = db.Companies.Find(companyId);

                var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
                
                // Check if the current users has rights, if not respond with bad request
                if (owner == null && manager == null) return BadRequest();

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

        [HttpGet]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("api/Companies/{companyId}/Drivers")]
        public IQueryable<DriverViewModel> GetDrivers(Guid companyId)
        {
            var result = new List<DriverViewModel>();

            var user = db.Users.Find(User.Identity.GetUserId());

            try
            {

				// get the company
				var company = db.Companies.Find(companyId);

				var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
				var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);
				var driver = company.Profiles.OfType<Driver>().FirstOrDefault(query => query.User.Email == user.Email);


                // Check if the current users has rights, if not respond with bad request
                if (owner != null || manager != null || accountant != null)
                {
                    var companyDrivers = company.Profiles.OfType<Driver>();
                    foreach (var d in companyDrivers)
                    {
						var driverInfo = db.DriverInfo.FirstOrDefault(info => info.DriverId == d.Id);
                        IList<VehicleViewModel> vehicles = db.VehicleInfo.Where(v => v.UserId.ToString() == d.User.Id.ToString()).Join(db.Vehicles, vehicleInfo => vehicleInfo.VehicleId, vehicle => vehicle.Id, (vehicleInfo, vehicle) => new { Vehicle = vehicle }).Select(v => new VehicleViewModel()
                        {
                            Id = v.Vehicle.Id.ToString(),
                            Registration = v.Vehicle.Registration,
                            EngineType = new EngineTypeViewModel()
                            {
                                Id = v.Vehicle.EngineType.Id.ToString(),
                                Name = v.Vehicle.EngineType.Name
                            },
                            VehicleType = new VehicleTypeViewModel()
                            {
                                Id = v.Vehicle.VehicleType.Id.ToString()
                            }
                        }).ToList();
                        result.Add(new DriverViewModel()
                        {
                            Id = d.Id.ToString(),
                            AutoAccept = driverInfo.AutoAccept,
                            FirstName = d.User.FirstName,
                            LastName = d.User.LastName,
                            Email = d.User.Email,
                            Vehicles = vehicles
                        });
                    }
                }
				else if (driver != null)
                {
                    var driverInfo = db.DriverInfo.FirstOrDefault(info => info.DriverId == driver.Id);
                    IList<VehicleViewModel> vehicles = db.VehicleInfo.Where(v => v.UserId.ToString() == driver.User.Id.ToString()).Join(db.Vehicles, vehicleInfo => vehicleInfo.VehicleId, vehicle => vehicle.Id, (vehicleInfo, vehicle) => new { Vehicle = vehicle }).Select(v => new VehicleViewModel()
                    {
                        Id = v.Vehicle.Id.ToString(),
                        Registration = v.Vehicle.Registration,
                        EngineType = new EngineTypeViewModel()
                        {
                            Id = v.Vehicle.EngineType.Id.ToString(),
                            Name = v.Vehicle.EngineType.Name
                        },
                        VehicleType = new VehicleTypeViewModel()
                        {
                            Id = v.Vehicle.VehicleType.Id.ToString()
                        }
                    }).ToList();
                    result.Add(new DriverViewModel()
                    {
                        Id = driver.Id.ToString(),
						AutoAccept = driverInfo.AutoAccept,
                        FirstName = driver.User.FirstName,
                        LastName = driver.User.LastName,
                        Email = driver.User.Email,
                        Vehicles = vehicles
                    });
                }
            }
            catch (Exception e)
            {                
                return null;
            }
            return result.AsQueryable();
        }

		[HttpGet]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("api/Companies/{companyId}/Driver/{driverId}")]
		public IQueryable<DriverViewModel> GetDriver(Guid companyId, Guid driverId)
		{
			var result = new List<DriverViewModel>();

			var user = db.Users.Find(User.Identity.GetUserId());

			try
			{

				// get the company
				var company = db.Companies.Find(companyId);

				if (company.Profiles.OfType<Driver>().FirstOrDefault(query => query.Id == driverId) == null)
				{
					return result.AsQueryable();
				}

				var driver = db.Drivers.Find(driverId);
				DriverInfo driverInfo = db.DriverInfo.FirstOrDefault(info => info.DriverId == driver.Id);

				var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
				var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);


				// Check if the current users has rights, if not respond with bad request
				if (owner != null || manager != null || accountant != null)
				{
					Company personal = db.Companies.FirstOrDefault(c => c.Name == "Personal" && c.Owner.User.Email == driver.User.Email);
					result.Add(new DriverViewModel()
					{
						Id = driver.Id.ToString(),
						AutoAccept = driverInfo.AutoAccept,
						FirstName = driver.User.FirstName,
						LastName = driver.User.LastName,
						Email = driver.User.Email,
						Vehicles = personal.Profiles.OfType<Driver>().FirstOrDefault().Vehicles.Select(v => new VehicleViewModel()
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
				else if (driver != null && driver.User.Email == user.Email)
				{
					Company personal = db.Companies.FirstOrDefault(c => c.Name == "Personal" && c.Owner.User.Email == driver.User.Email);
					if (personal != null)
					{
						result.Add(new DriverViewModel()
						{
							Id = driver.Id.ToString(),
							AutoAccept = driverInfo.AutoAccept,
							FirstName = driver.User.FirstName,
							LastName = driver.User.LastName,
							Email = driver.User.Email,
							Vehicles = personal.Profiles.OfType<Driver>().FirstOrDefault().Vehicles.Select(v => new VehicleViewModel()
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
			catch (Exception)
			{
				return null;
			}
			return result.AsQueryable();
		}


        [HttpPost]
        [Route("api/Companies/{companyId}/AddDriver")]
        public async Task<IHttpActionResult> AddCompanyDriver(Guid companyId, DriverBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // get the company
            var company = db.Companies.Find(companyId);

            try
            {
				// get the company owners and managers
				var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);

                // Check if the current users has rights, if not respond with bad request
                if (manager == null && owner == null) return BadRequest();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // get the profile of the driver to be added
            var newDriver = db.Profiles.Find(Guid.Parse(model.Id)) as Driver;

            // If the driver doesnt exist then return BadRequest
            if (newDriver == null) return BadRequest();

            db.DriverInfo.Add(new DriverInfo()
            {
                Id = Guid.NewGuid(),
                DriverId = newDriver.Id,
                CompanyId = company.Id,
                CurrentMileage = model.CurrentMileage,
                AutoAccept = false
			});

            try
            {
                // Add the driver to the company
                company.Profiles.Add(newDriver);
                user.Profiles.Add(newDriver);
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

            // get the company
            var company = db.Companies.Find(companyId);

			if (company.Profiles.OfType<Driver>().FirstOrDefault(query => query.Id == driverId) == null)
			{
				return BadRequest();
			}

            try
            {
                var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);

                // Check if the current users has rights, if not respond with bad request
                if (manager == null && owner == null) return BadRequest();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            // Get the driver were removing
            var driver = db.Drivers.Find(driverId);

			var driverInfo = db.DriverInfo.FirstOrDefault(info => info.DriverId == driver.Id);

            try
            {
                // If the driver isnt part of the company return bad request
                if (!company.Profiles.Contains(driver)) return BadRequest();

                // Remove the driver from the company
                company.Profiles.Remove(driver);
				db.DriverInfo.Remove(driverInfo);
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

		[HttpPost]
		[ResponseType(typeof(DriverViewModel))]
		[Route("api/Companies/{companyId}/DriverAutoAccept/{driverId}/{value}")]
		public async Task<IHttpActionResult> CompanyDriverAutoAccept(Guid companyId, Guid driverId, String value)
		{
			// Get the current User
			var user = db.Users.Find(User.Identity.GetUserId());

			// get the company
			var company = db.Companies.Find(companyId);

			if (company.Profiles.OfType<Driver>().FirstOrDefault(query => query.Id == driverId) == null)
			{
				return BadRequest();
			}

			//TODO redo the rights system

			try
			{
				var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
				var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);

				// Check if the current users has rights, if not respond with bad request
				if (manager == null && owner == null) return BadRequest();
			}
			catch (NullReferenceException e)
			{
				Console.WriteLine(e);
			}

			var driverInfo = db.DriverInfo.FirstOrDefault(info => info.DriverId == driverId);

			if (driverInfo != null)
			{
				driverInfo.AutoAccept = value.ToLower().Equals("true");
			}

			// Save changes
			await db.SaveChangesAsync();

			return Ok();
		}

		[HttpGet]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("api/Companies/{companyId}/JourneysInfo")]
		public IQueryable<JourneyInfoViewModel> getJourneysInfo(Guid companyId, long start, long end)
		{
			var result = new List<JourneyInfoViewModel>();
			DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			startDate = startDate.AddMilliseconds(start).ToLocalTime();
			DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			endDate = endDate.AddMilliseconds(end).ToLocalTime();

			try
			{
				Company company = db.Companies.Find(companyId);

				if (company != null)
				{
					var user = db.Users.Find(User.Identity.GetUserId());

					var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
					var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
					var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);
					var driver = company.Profiles.OfType<Driver>().FirstOrDefault(query => query.User.Email == user.Email);

					if (owner != null || manager != null || accountant != null)
					{
						var journeys = company.Journeys.Where(journey => journey.Company.Id == company.Id && journey.Date >= startDate && journey.Date <= endDate);
						foreach (var journey in journeys)
						{
							result.Add(new JourneyInfoViewModel()
							{
								Id = journey.Id,
								Date = journey.Date
							});
						}
					}
					else if (driver != null)
					{
						var journeys = company.Journeys.Where(journey => journey.Company.Id == company.Id && journey.Driver.Id == driver.Id && journey.Date >= startDate && journey.Date <= endDate);
						foreach (var journey in journeys)
						{
							result.Add(new JourneyInfoViewModel()
							{
								Id = journey.Id,
								Date = journey.Date
							});
						}
					}
				}
			} catch (NullReferenceException e)
			{
				Console.WriteLine(e);
			}
			return result.AsQueryable();
		}

		[HttpGet]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("api/Companies/{companyId}/DriverJourneysInfo/{driverId}")]
		public IQueryable<JourneyInfoViewModel> getDriverJourneysInfo(Guid companyId, Guid driverId, long start, long end)
		{
			var result = new List<JourneyInfoViewModel>();
			DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			startDate = startDate.AddMilliseconds(start).ToLocalTime();
			DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			endDate = endDate.AddMilliseconds(end).ToLocalTime();

			try
			{
				Company company = db.Companies.Find(companyId);

				if (company.Profiles.OfType<Driver>().FirstOrDefault(query => query.Id == driverId) == null)
				{
					return result.AsQueryable();
				}


				if (company != null)
				{
					var user = db.Users.Find(User.Identity.GetUserId());

					var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
					var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
					var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);
					var driver = company.Profiles.OfType<Driver>().FirstOrDefault(query => query.User.Email == user.Email);

					if (owner != null || manager != null || accountant != null)
					{
						var journeys = company.Journeys.Where(journey => journey.Company.Id == company.Id && journey.Driver.Id == driver.Id && journey.Date >= startDate && journey.Date <= endDate);
						foreach (var journey in journeys)
						{
							result.Add(new JourneyInfoViewModel()
							{
								Id = journey.Id,
								Date = journey.Date
							});
						}
					}
					else if (driver != null && driver.User.Email == user.Email)
					{
						var journeys = company.Journeys.Where(journey => journey.Company.Id == company.Id && journey.Driver.Id == driver.Id && journey.Date >= startDate && journey.Date <= endDate);
						foreach (var journey in journeys)
						{
							result.Add(new JourneyInfoViewModel()
							{
								Id = journey.Id,
								Date = journey.Date
							});
						}
					}
				}
			}
			catch (NullReferenceException e)
			{
				Console.WriteLine(e);
			}
			return result.AsQueryable();
		}

		[HttpGet]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("api/Companies/{companyId}/LazyJourneys")]
		public IQueryable<JourneyViewModel> getJourneys(Guid companyId, String info)
		{
			var result = new List<JourneyViewModel>();
			try {
				Company company = db.Companies.Find(companyId);

				if (company != null)
				{
					var user = db.Users.Find(User.Identity.GetUserId());

					var owner = company.Profiles.OfType<Owner>().FirstOrDefault(query => query.User.Email == user.Email);
					var manager = company.Profiles.OfType<Manager>().FirstOrDefault(query => query.User.Email == user.Email);
					var accountant = company.Profiles.OfType<Accountant>().FirstOrDefault(query => query.User.Email == user.Email);
					var driver = company.Profiles.OfType<Driver>().FirstOrDefault(query => query.User.Email == user.Email);

					if (owner != null || manager != null || accountant != null || driver != null)
					{
						if (info.IndexOf(',') == -1)
						{
							if (owner == null && manager == null && accountant == null && driver != null)
							{
								var journey = company.Journeys.FirstOrDefault(query => query.Id.ToString() == info && query.Company.Id == company.Id && query.Driver.Id == driver.Id);
								if (journey != null)
								{
									result.Add(new JourneyViewModel()
									{
										Accepted = journey.Accepted,
										Company = new CompanyViewModel()
										{
											Id = journey.Company.Id.ToString()
										},
										Cost = Convert.ToDouble(journey.Cost),
										Date = journey.Date,
                                        Modified = journey.Modified,
										Driver = new DriverViewModel()
										{
											Id = journey.Driver.Id.ToString(),
											Email = journey.Driver.User.Email,
											FirstName = journey.Driver.User.FirstName,
											LastName = journey.Driver.User.LastName
										},
										Vehicle = new VehicleViewModel()
										{
											Id = journey.Vehicle.Id.ToString(),
											Registration = journey.Vehicle.Registration,
											EngineType = new EngineTypeViewModel()
											{
												Id = journey.Vehicle.EngineType.Id.ToString(),
												Name = journey.Vehicle.EngineType.Name
											}
										},
										Distance = journey.Distance,
										Id = journey.Id.ToString(),
										Invoiced = journey.Invoiced,
										Passengers = journey.Passengers,
										Reason = journey.Reason,
										Rejected = journey.Rejected,
                                        Waypoints = journey.Waypoints.Count
									});
								}
							}
							else
							{
								var journey = company.Journeys.FirstOrDefault(query => query.Id.ToString() == info && query.Company.Id == company.Id);
								if (journey != null)
								{
									result.Add(new JourneyViewModel()
									{
										Accepted = journey.Accepted,
										Company = new CompanyViewModel()
										{
											Id = journey.Company.Id.ToString()
										},
										Cost = Convert.ToDouble(journey.Cost),
										Date = journey.Date,
                                        Modified = journey.Modified,
										Driver = new DriverViewModel()
										{
											Id = journey.Driver.Id.ToString(),
											Email = journey.Driver.User.Email,
											FirstName = journey.Driver.User.FirstName,
											LastName = journey.Driver.User.LastName
										},
										Vehicle = new VehicleViewModel()
										{
											Id = journey.Vehicle.Id.ToString(),
											Registration = journey.Vehicle.Registration,
											EngineType = new EngineTypeViewModel()
											{
												Id = journey.Vehicle.EngineType.Id.ToString(),
												Name = journey.Vehicle.EngineType.Name
											}
										},
										Distance = journey.Distance,
										Id = journey.Id.ToString(),
										Invoiced = journey.Invoiced,
										Passengers = journey.Passengers,
										Reason = journey.Reason,
										Rejected = journey.Rejected,
                                        Waypoints = journey.Waypoints.Count
                                    });
								}
							}
						}
						else
						{
							var ids = info.Split(',');
						    if (owner == null && manager == null && accountant == null && driver != null)
						    {
								foreach (var id in ids)
								{
									var journey = company.Journeys.FirstOrDefault(query => query.Id.ToString() == id && query.Company.Id == company.Id && query.Driver.Id == driver.Id);
									if (journey != null)
									{
										result.Add(new JourneyViewModel()
										{
											Accepted = journey.Accepted,
											Company = new CompanyViewModel()
											{
												Id = journey.Company.Id.ToString()
											},
											Cost = Convert.ToDouble(journey.Cost),
											Date = journey.Date,
                                            Modified = journey.Modified,
											Driver = new DriverViewModel()
											{
												Id = journey.Driver.Id.ToString(),
												Email = journey.Driver.User.Email,
												FirstName = journey.Driver.User.FirstName,
												LastName = journey.Driver.User.LastName
											},
											Vehicle = new VehicleViewModel()
											{
												Id = journey.Vehicle.Id.ToString(),
												Registration = journey.Vehicle.Registration,
												EngineType = new EngineTypeViewModel()
												{
													Id = journey.Vehicle.EngineType.Id.ToString(),
													Name = journey.Vehicle.EngineType.Name
												}
											},
											Distance = journey.Distance,
											Id = journey.Id.ToString(),
											Invoiced = journey.Invoiced,
											Passengers = journey.Passengers,
											Reason = journey.Reason,
											Rejected = journey.Rejected,
                                            Waypoints = journey.Waypoints.Count
                                        });
									}
								}
							}
							else
							{
							    foreach (var id in ids)
							    {
							        var journey =
							            company.Journeys.FirstOrDefault(query => query.Id.ToString() == id && query.Company.Id == company.Id);
							        if (journey != null)
							        {
							            result.Add(new JourneyViewModel()
							            {
							                Accepted = journey.Accepted,
							                Company = new CompanyViewModel()
							                {
							                    Id = journey.Company.Id.ToString()
							                },
							                Cost = Convert.ToDouble(journey.Cost),
							                Date = journey.Date,
                                            Modified = journey.Modified,
                                            Driver = new DriverViewModel()
							                {
							                    Id = journey.Driver.Id.ToString(),
							                    Email = journey.Driver.User.Email,
							                    FirstName = journey.Driver.User.FirstName,
							                    LastName = journey.Driver.User.LastName
							                },
							                Vehicle = new VehicleViewModel()
							                {
							                    Id = journey.Vehicle.Id.ToString(),
							                    Registration = journey.Vehicle.Registration,
							                    EngineType = new EngineTypeViewModel()
							                    {
							                        Id = journey.Vehicle.EngineType.Id.ToString(),
							                        Name = journey.Vehicle.EngineType.Name
							                    }
							                },
							                Distance = journey.Distance,
							                Id = journey.Id.ToString(),
							                Invoiced = journey.Invoiced,
							                Passengers = journey.Passengers,
							                Reason = journey.Reason,
							                Rejected = journey.Rejected,
                                            Waypoints = journey.Waypoints.Count
                                        });
							        }
							    }
							}
						}
					}
				}
			}
			catch (NullReferenceException e)
			{
				Console.WriteLine(e);
			}
			return result.AsQueryable();
		}

        [Route("api/Companies/{companyId}/update")]
        public async Task<IHttpActionResult> PostUpdateCompanyInfo(Guid companyId, CompanyViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Get the current User
                var userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                // Checks if the posted company exists in the database
                var company = db.Companies.FirstOrDefault(c => c.Id == companyId);
                //var company = db.Companies.FirstOrDefault(c => c.Id == model.Id);

                if (company == null)
                {
                    return BadRequest();
                }

                company.Name = model.Name;
                company.LowRate = model.LowRate;
                company.HighRate = model.HighRate;

                await db.SaveChangesAsync();

                return Ok();

            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception in Companies Controller, updateCompanyInfo: " + e);
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