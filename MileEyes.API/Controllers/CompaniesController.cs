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
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Driver;
using MileEyes.PublicModels.Journey;

namespace MileEyes.API.Controllers
{
    [Authorize]
    public class CompaniesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Companies
        public IQueryable<CompanyViewModel> GetCompanies()
        {
            return db.Companies.Select(c => new CompanyViewModel()
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Personal = c.Personal,
            });
        }

        // GET: api/Companies/5
        [ResponseType(typeof(CompanyViewModel))]
        public async Task<IHttpActionResult> GetCompany(Guid id)
        {
            Company company = await db.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(new CompanyViewModel()
            {
                Id = company.Id.ToString(),
                Name = company.Name,
                LowRate = company.LowRate,
                HighRate = company.HighRate,
                Personal = company.Personal,
            });
        }

        [Route("Companies/{companyId}/Journeys")]
        public IQueryable<JourneyViewModel> GetJourneys(Guid companyId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            var driver = user.Profiles.OfType<Driver>().FirstOrDefault();

            return db.Journeys.Where(j => j.Company.Id == companyId).Select(j => new JourneyViewModel()
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
                    LastName = j.Driver.User.LastName
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

        [Route("Companies/{companyId}/AddDriver")]

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