using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MileEyes.PublicModels.EngineTypes;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class EngineTypeService : IEngineTypeService
    {
        public static bool EngineTypeSyncing;

        public event EventHandler SyncFailed = delegate { };

        public async Task<IEnumerable<EngineType>> GetEngineTypes()
        {
            var engineTypes = DatabaseService.Realm.All<EngineType>();

            if (engineTypes.Any())
            {
                return engineTypes;
            }

            await Sync();

            engineTypes = DatabaseService.Realm.All<EngineType>();

            return engineTypes;
        }

        public async Task Sync()
        {
            if (!TrackerService.IsTracking && !JourneyService.JourneySyncing && !VehicleService.VehicleSyncing && !CompanyService.CompanySyncing && !VehicleTypeService.VehicleTypeSyncing)
            {
                EngineTypeSyncing = true;
                var response = await RestService.Client.GetAsync("api/EngineTypes/");

                if (response == null) { SyncFailed?.Invoke(this, EventArgs.Empty); return; }

                if (!response.IsSuccessStatusCode)
                {
                    EngineTypeSyncing = false;
                    SyncFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                var result =
                    JsonConvert.DeserializeObject<IEnumerable<EngineTypeViewModel>>(
                        await response.Content.ReadAsStringAsync());

                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    DatabaseService.Realm.RemoveAll<EngineType>();

                    foreach (var et in result)
                    {
                        var realmEt = DatabaseService.Realm.CreateObject<EngineType>();
                        realmEt.Id = et.Id;
                        realmEt.Name = et.Name;
                    }

                    transaction.Commit();
                    transaction.Dispose();
                }
                EngineTypeSyncing = false;
            }
        }
    }
}