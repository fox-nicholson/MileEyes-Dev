using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IVehicleTypeService
    {
        event EventHandler SyncFailed;

        Task<IEnumerable<VehicleType>> GetVehicleTypes();

        Task Sync();
    }
}