using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models
{
    public class CostModel
    {
        public double Distance { get; set; }
        public double UnderDistance { get; set; }
        public double OverDistance { get; set; }
        public decimal Cost { get; set; }
        public decimal UnderCost { get; set; }
        public decimal OverCost { get; set; }
    }
}