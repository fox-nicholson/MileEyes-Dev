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
                Id = c.Id.ToString()
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