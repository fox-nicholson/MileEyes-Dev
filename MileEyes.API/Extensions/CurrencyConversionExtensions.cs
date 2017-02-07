using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions
{
    public static class CurrencyConversionExtensions
    {
        public static decimal ConvertToCurrency(this decimal amount, CurrencyRate type)
        {
            return amount * type.Rate;
        }

        public static decimal ConvertFromCurrency(this decimal amount, CurrencyRate type)
        {
            return amount / type.Rate;
        }
    }
}