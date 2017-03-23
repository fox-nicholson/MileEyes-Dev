using System;

namespace MileEyes.PublicModels.Company
{
    public class CompanyBindingModel
    {
        public string Id { get; set; }
        public string CloudId { get; set; }
        public string Name { get; set; }
        public decimal LowRate { get; set; }
        public decimal HighRate { get; set; }
    }
}