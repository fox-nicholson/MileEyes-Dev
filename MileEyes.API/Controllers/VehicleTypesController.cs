using System;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using MileEyes.API.Models;
using MileEyes.PublicModels.VehicleTypes;

namespace MileEyes.API.Controllers
{
    public class VehicleTypesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/VehicleTypes
        [EnableQuery]
        public IQueryable<VehicleTypeViewModel> GetVehicleTypes()
        {
            try
            {
                return db.VehicleTypes.Select(et => new VehicleTypeViewModel()
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

        private bool VehicleTypeExists(Guid id)
        {
            return db.VehicleTypes.Count(e => e.Id == id) > 0;
        }
    }
}