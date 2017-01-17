using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.AccountingAdapters
{
    public class FreeagentAdapter : IAccountingAdapter
    {
        public async Task<string> PostMileageExpense(string accessToken, Journey journey)
        {
            throw new NotImplementedException();
        }
    }
}