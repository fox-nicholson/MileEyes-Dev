using System;

namespace MileEyes.PublicModels.Company
{
    public class CompanyViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Personal { get; set; }

        public decimal LowRate { get; set; }

        public decimal HighRate { get; set; }

        public DateTimeOffset StartOfTaxYear { get; set; }
    }
}