using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.PublicModels.Company
{
    public class CompanyBindingModel
    {
        public string Id { get; set; }
        public string CloudId { get; set; }
        public string Name { get; set; }
        public string PlaceId { get; set; }
        public bool Personal { get; set; }
        public bool AutoAccept { get; set; }
        public double AutoAcceptDistance { get; set; }
        public bool Vat { get; set; }
        public string VatNumber { get; set; }
        public decimal LowRate { get; set; }
        public decimal HighRate { get; set; }
        public double CurrentMileage { get; set; }
        public decimal CurrentExpenses { get; set; }
    }
}
