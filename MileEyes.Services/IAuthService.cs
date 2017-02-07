using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IAuthService
    {
        bool Authenticated { get; }
        void Logout();
        Task<AuthResponse> Authenticate(string email, string password);
        Task<RegisterResponse> Register(string firstName, string lastName, string email, string password, string confirmPassword, string addressPlaceId);
    }
}