using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IVehicleService
    {
        Task<IQueryable<Vehicle>> GetVehicles();
        Task<IQueryable<Vehicle>> GetAllVehicles();

        Task<Vehicle> GetVehicle(string id);

        Task<Vehicle> AddVehicle(Vehicle v);

        Task<Vehicle> RemoveVehicle(string id);

        Task SetDefault(string id);

        Task Sync();
    }
}