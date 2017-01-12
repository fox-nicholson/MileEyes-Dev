using System.Threading.Tasks;
using MileEyes.PublicModels;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IUserService
    {
        Task<UserInfoResponse> UpdateDetails(string firstName, string lastName, string email, string addressPlaceId);

        Task<UserInfoResponse> GetDetails();
    }
}