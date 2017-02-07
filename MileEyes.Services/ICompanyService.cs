using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<Company>> GetCompanies();
        Task<Company> GetCompany(string id);

        Task Sync();

        Task SetDefault(string id);
    }
}