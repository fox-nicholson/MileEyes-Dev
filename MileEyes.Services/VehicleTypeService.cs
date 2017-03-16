using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MileEyes.PublicModels.VehicleTypes;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class VehicleTypeService : IVehicleTypeService
    {
        public static bool VehicleTypeSyncing;
        public event EventHandler SyncFailed = delegate { };

        public async Task<IEnumerable<VehicleType>> GetVehicleTypes()
        {
            var vehicleTypes = DatabaseService.Realm.All<VehicleType>();

            if (vehicleTypes.Any())
            {
                return vehicleTypes;
            }

            await Sync();

            vehicleTypes = DatabaseService.Realm.All<VehicleType>();

            return vehicleTypes;
        }

        public async Task Sync()
        {
            if (!TrackerService.IsTracking && !JourneyService.JourneySyncing && !VehicleService.VehicleSyncing && !CompanyService.CompanySyncing && !EngineTypeService.EngineTypeSyncing)
            {
                VehicleTypeSyncing = true;
                var response = await RestService.Client.GetAsync("api/VehicleTypes/");

                if (response == null) { SyncFailed?.Invoke(this, EventArgs.Empty); return; }

                if (!response.IsSuccessStatusCode)
                {
                    VehicleTypeSyncing = false;
                    SyncFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                var result =
                    JsonConvert.DeserializeObject<IEnumerable<VehicleTypeViewModel>>(
                        await response.Content.ReadAsStringAsync());

                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    DatabaseService.Realm.RemoveAll<VehicleType>();

                    foreach (var et in result)
                    {
                        var realmEt = DatabaseService.Realm.CreateObject<VehicleType>();
                        realmEt.Id = et.Id;
                        realmEt.Name = et.Name;
                    }

                    transaction.Commit();
                    transaction.Dispose();
                }
                VehicleTypeSyncing = false;
            }
        }
    }
}