using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace MileEyes.Services.Models
{
    public class Company : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string CloudId { get; set; }

        public string Name { get; set; }

        public bool Personal { get; set; }
        public bool Default { get; set; }
    }
}
