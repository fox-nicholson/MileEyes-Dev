using System;
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

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class CompaniesController : ApiController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Companies
        public IQueryable<CompanyViewModel> GetCompanies()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            return driver.Companies.Select(c => new CompanyViewModel
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Personal = c.Personal
            }).AsQueryable();
        }

        [ResponseType(typeof(CompanyViewModel))]
        public async Task<IHttpActionResult> PostCompany(CompanyBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner profile
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var accountant = user.Profiles.OfType<Accountant>().FirstOrDefault();

            // Get existing addresses with the same PlaceId
            var existingAddress = db.Addresses.FirstOrDefault(a => a.PlaceId == model.PlaceId);

            Address address;

            // Is there any addresses?
            if (existingAddress == null)
            {
                // No so we need to check with Google and get the details
                var addressResult = await GeocodingService.GetAddress(model.PlaceId);

                // Create the new Address in database
                address = new Address
                {
                    Id = Guid.NewGuid(),
                    PlaceId = addressResult.PlaceId,
                    Coordinates = new Coordinates
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

            // Setup the new company
            var newCompany = new Company
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

            // Add the owners profiles to the company
            newCompany.Profiles.Add(driver);
            newCompany.Profiles.Add(manager);
            newCompany.Profiles.Add(accountant);

            // Save the changes
            await db.SaveChangesAsync();

            // Return the new company's details
            return Ok(new CompanyViewModel
            {
                Id = newCompany.Id.ToString(),
                Name = model.Name,
                HighRate = model.HighRate,
                LowRate = model.LowRate,
                Personal = false
            });
        }

        // GET: api/Companies/5
        [ResponseType(typeof(CompanyViewModel))]
        public async Task<IHttpActionResult> GetCompany(Guid id)
        {
            var company = await db.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            return Ok(new CompanyViewModel
            {
                Id = company.Id.ToString(),
                Name = company.Name,
                LowRate = company.LowRate,
                HighRate = company.HighRate,
                Personal = company.Personal
            });
        }

        [Route("Companies/{companyId}/Journeys")]
        public IQueryable<JourneyViewModel> GetJourneys(Guid companyId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            return db.Journeys.Where(j => j.Company.Id == companyId).Select(j => new JourneyViewModel
            {
                Accepted = j.Accepted,
                Company = new CompanyViewModel
                {
                    Id = j.Company.Id.ToString()
                },
                Cost = Convert.ToDouble(j.Cost),
                Date = j.Date,
                Driver = new DriverViewModel
                {
                    Id = j.Driver.Id.ToString(),
                    FirstName = j.Driver.User.FirstName,
                    LastName = j.Driver.User.LastName
                },
                Distance = j.Distance,
                Id = j.Id.ToString(),
                Invoiced = j.Invoiced,
                Passengers = j.Passengers,
                Reason = j.Reason,
                Rejected = j.Rejected,
                Waypoints = j.Waypoints.Select(w => new WaypointViewModel
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

        [Route("Companies/{companyId}/Journeys/{journeyId}/Accept")]
        public async Task<IHttpActionResult> AcceptCompanyJourney(Guid companyId, Guid journeyId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            // get the company owners and managers
            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            // Check if the current users has rights, if not respond with bad request
            if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

            var journey = db.Journeys.Find(journeyId);

            if (!company.Journeys.Contains(journey)) return BadRequest();

            journey.Accepted = true;
            journey.Rejected = false;

            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("Companies/{companyId}/Journeys/{journeyId}/Reject")]
        public async Task<IHttpActionResult> RejectCompanyJourney(Guid companyId, Guid journeyId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            // get the company owners and managers
            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            // Check if the current users has rights, if not respond with bad request
            if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

            var journey = db.Journeys.Find(journeyId);

            if (!company.Journeys.Contains(journey)) return BadRequest();

            journey.Accepted = false;
            journey.Rejected = true;

            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("Companies/{companyId}/Journeys/{journeyId}/PushToAccountancy/{accountingPackage}")]
        public async Task<IHttpActionResult> PushCompanyJourneyToAccouncy(Guid companyId, Guid journeyId,
            string accountancyPackage)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            // get the company owners and managers
            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            // Check if the current users has rights, if not respond with bad request
            if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();


            /*
             * Implement accounting push here using respective API's, Fox has links to the relevant docs for this
             */


            // Always returns Bad until implemented
            return BadRequest();
        }

        [HttpPost]
        [Route("Companies/{companyId}/AddDriver")]
        public async Task<IHttpActionResult> AddCompanyDriver(Guid companyId, DriverBindingModel model)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            // get the company owners and managers
            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            // Check if the current users has rights, if not respond with bad request
            if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

            // get the profile of the driver to be added
            var newDriver = db.Profiles.Find(Guid.Parse(model.Id)) as Driver;

            // If the driver doesnt exist then return BadRequest
            if (newDriver == null) return BadRequest();

            // Add the driver to the company
            company.Profiles.Add(newDriver);

            // Save the changes
            await db.SaveChangesAsync();

            // Return the driver so that client/browser can show success
            return Ok(new DriverViewModel
            {
                Id = newDriver.Id.ToString()
            });
        }

        [HttpPost]
        [ResponseType(typeof(DriverViewModel))]
        [Route("Companies/{companyId}/RemoveDriver/{driverId}")]
        public async Task<IHttpActionResult> RemoveCompanyDriver(Guid companyId, Guid driverId)
        {
            // Get the current User
            var user = db.Users.Find(User.Identity.GetUserId());

            // Get the users owner and manager profiles
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();

            // get the company
            var company = db.Companies.Find(companyId);

            // get the company owners and managers
            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            // Check if the current users has rights, if not respond with bad request
            if (!companyManagers.Contains(manager) && !companyOwners.Contains(owner)) return BadRequest();

            // Get the driver were removing
            var driver = db.Drivers.Find(driverId);

            // If the driver isnt part of the company return bad request
            if (!company.Profiles.Contains(driver)) return BadRequest();

            // Remove the driver from the company
            company.Profiles.Remove(driver);

            // Save changes
            await db.SaveChangesAsync();

            // Return the drivers Id to show complete
            return Ok(new DriverViewModel
            {
                Id = driver.Id.ToString()
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}