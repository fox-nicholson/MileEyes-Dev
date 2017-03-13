﻿using System;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using MileEyes.API.Models;
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
            try
            {
                return db.EngineTypes.Select(et => new EngineTypeViewModel()
                {
                    Id = et.Id.ToString(),
                    Name = et.Name,
                });
            }catch (Exception)
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

        private bool EngineTypeExists(Guid id)
        {
            return db.EngineTypes.Count(e => e.Id == id) > 0;
        }
    }
}