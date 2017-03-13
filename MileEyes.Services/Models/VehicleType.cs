using Realms;
using System;

namespace MileEyes.Services.Models
{
    public class VehicleType : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string CloudId { get; set; }
        public string Name { get; set; }
    }
}