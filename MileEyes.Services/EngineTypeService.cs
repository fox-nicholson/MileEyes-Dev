using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.EngineTypes;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    class EngineTypeService : IEngineTypeService
    {
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
            try
            {
                var response = await RestService.Client.GetAsync("api/EngineTypes/");

                if (!response.IsSuccessStatusCode)
                {
                    SyncFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                var result =
                    JsonConvert.DeserializeObject<IEnumerable<EngineTypeViewModel>>(await response.Content.ReadAsStringAsync());

                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    DatabaseService.Realm.RemoveAll<EngineType>();

                    foreach (var et in result)
                    {
                        var realmEt = DatabaseService.Realm.CreateObject<EngineType>();
                        realmEt.Id = et.Id.ToString();
                        realmEt.Name = et.Name;
                    }

                    transaction.Commit();
                }
            }
            catch
            {
                SyncFailed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
