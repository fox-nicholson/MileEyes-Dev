using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.AccountingAdapters
{
    public interface IAccountingAdapter
    {
        Task<string> PostMileageExpense(string accessToken, Journey journey);
    }
}