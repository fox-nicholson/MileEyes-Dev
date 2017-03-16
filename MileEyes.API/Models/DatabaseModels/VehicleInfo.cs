using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class VehicleInfo
    {
        public Guid Id { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual Guid VehicleId { get; set; }
    }
}