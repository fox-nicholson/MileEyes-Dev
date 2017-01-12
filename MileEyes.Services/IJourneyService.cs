using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IJourneyService
    {
        Task<IEnumerable<Journey>> GetJourneys();

        Task<Journey> GetJourney(string id);

        Task<Journey> SaveJourney(Journey j);

        Task<Journey> DeleteJourney(string id);

        Task Sync();
    }
}