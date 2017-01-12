using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class CurrencyRate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Rate { get; set; }
    }
}