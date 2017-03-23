using System;

namespace MileEyes.PublicModels.Company
{
    public class CompanyViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public decimal LowRate { get; set; }

        public decimal HighRate { get; set; }

		public int rank { get; set; }
    }
}