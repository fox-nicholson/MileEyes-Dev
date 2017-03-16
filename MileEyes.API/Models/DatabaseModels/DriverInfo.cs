using System;

namespace MileEyes.API.Models.DatabaseModels
{
    public class DriverInfo
    {
        public Guid Id { get; set; }
        public virtual Guid DriverId { get; set; }
        public virtual Guid CompanyId { get; set; }
        public double CurrentMileage { get; set; }
    }
}