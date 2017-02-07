using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IReasonService
    {
        Task<IEnumerable<Reason>> GetReasons();

        Task<Reason> GetReason(string id);

        Task<Reason> SaveReason(Reason r);

        Task<Reason> DeleteReason(string id);

        Task SetDefault(string id);
    }
}