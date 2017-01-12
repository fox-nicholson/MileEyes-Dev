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
using System.Web.Http.OData;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.PublicModels.EngineTypes;

namespace MileEyes.API.Controllers
{
    public class EngineTypesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/EngineTypes
        [EnableQuery]
        public IQueryable<EngineTypeViewModel> GetEngineTypes()
        {
            return db.EngineTypes.Select(et => new EngineTypeViewModel()
            {
                Id = et.Id.ToString(),
                Name = et.Name,
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

        private bool EngineTypeExists(Guid id)
        {
            return db.EngineTypes.Count(e => e.Id == id) > 0;
        }
    }
}