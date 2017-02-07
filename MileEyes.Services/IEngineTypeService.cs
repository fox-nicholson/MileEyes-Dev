using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IEngineTypeService
    {
        event EventHandler SyncFailed;

        Task<IEnumerable<EngineType>> GetEngineTypes();

        Task Sync();
    }
}