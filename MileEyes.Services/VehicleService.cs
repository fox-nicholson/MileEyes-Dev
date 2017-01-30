using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Vehicles;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class VehicleService : IVehicleService
    {
        public async Task<Vehicle> AddVehicle(Vehicle v)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var vehicle = DatabaseService.Realm.CreateObject<Vehicle>();

                var engineType =
                    DatabaseService.Realm.All<EngineType>().FirstOrDefault(et => et.Id == v.EngineType.Id);

                vehicle.Id = Guid.NewGuid().ToString();
                vehicle.CloudId = v.CloudId;
                vehicle.Registration = v.Registration.ToUpper();
                vehicle.EngineType = engineType;

                transaction.Commit();

                return vehicle;
            }
        }

        public async Task<Vehicle> GetVehicle(string id)
        {
            return DatabaseService.Realm.ObjectForPrimaryKey<Vehicle>(id);
        }

        public async Task<IQueryable<Vehicle>> GetVehicles()
        {
            return DatabaseService.Realm.All<Vehicle>().Where(v => v.MarkedForDeletion == false);
        }

        public async Task<IQueryable<Vehicle>> GetAllVehicles()
        {
            return DatabaseService.Realm.All<Vehicle>();
        }

        public async Task<Vehicle> RemoveVehicle(string id)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var vehicle = await GetVehicle(id);

                var journeys = (await Host.JourneyService.GetJourneys()).ToArray();
                var vehiclesJourneys = journeys.Where(j => j.Vehicle.Id == vehicle.Id);

                if (!vehiclesJourneys.Any())
                {
                    if (!string.IsNullOrEmpty(vehicle.CloudId))
                    {
                        vehicle.MarkedForDeletion = true;
                    }
                    else
                    {
                        DatabaseService.Realm.Remove(vehicle);
                    }

                    transaction.Commit();

                    return vehicle;
                }

                return null;
            }
        }

        public async Task SetDefault(string id)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var vehicles = DatabaseService.Realm.All<Vehicle>();

                foreach (var vehicle in vehicles)
                {
                    vehicle.Default = vehicle.Id == id;
                }

                transaction.Commit();
            }
        }

        public async Task Sync()
        {
            await PushNew();
            await PullUpdate();
            await DeleteMarked();
        }

        private async Task DeleteMarked()
        {
            var vehicles = await GetAllVehicles();

            var temp = vehicles;

            foreach (var v in vehicles.Where(v => v.MarkedForDeletion == true))
            {
                try
                {
                    var response = await RestService.Client.DeleteAsync("/api/Vehicles/" + v.CloudId);

                    if (!response.IsSuccessStatusCode) continue;

                    //var result =
                    //    JsonConvert.DeserializeObject<VehicleViewModel>(await response.Content.ReadAsStringAsync());

                    //if (result == null) continue;

                    using (var transaction = DatabaseService.Realm.BeginWrite())
                    {
                        var vehicleToRemove = await GetVehicle(v.Id);
                        DatabaseService.Realm.Remove(vehicleToRemove);

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
        }

        private async Task PullUpdate()
        {
            try
            {
                var response = await RestService.Client.GetAsync("/api/Vehicles/");

                if (!response.IsSuccessStatusCode) return;

                var result =
                    JsonConvert.DeserializeObject<ICollection<VehicleViewModel>>(await response.Content.ReadAsStringAsync());

                foreach (var vehicleData in result)
                {
                    var vehicles = await GetVehicles();

                    var vehiclesEnumerable = vehicles.ToArray();

                    var existingVehicles = vehiclesEnumerable.Where(v => v.CloudId == vehicleData.Id);

                    var existingVehiclesEnumerable = existingVehicles as Vehicle[] ?? existingVehicles.ToArray();

                    if (!existingVehiclesEnumerable.Any())
                    {
                        var vehicle = new Vehicle();

                        var engineTypes = await Host.EngineTypeService.GetEngineTypes();

                        var engineType = engineTypes.FirstOrDefault(et => et.Id == vehicleData.EngineType.Id);

                        vehicle.CloudId = vehicleData.Id;
                        vehicle.Registration = vehicleData.Registration;
                        vehicle.EngineType = engineType;

                        await AddVehicle(vehicle);

                        return;
                    }

                    var existingVehicle = await GetVehicle(existingVehiclesEnumerable.FirstOrDefault().Id);

                    using (var transaction = DatabaseService.Realm.BeginWrite())
                    {
                        var engineTypes = await Host.EngineTypeService.GetEngineTypes();

                        var engineType = engineTypes.FirstOrDefault(
                                et => et.Id == vehicleData.EngineType.Id);

                        var a = engineType;

                        existingVehicle.EngineType = engineType;
                        existingVehicle.Registration = vehicleData.Registration;

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }

        private async Task PushNew()
        {
            var vehicles = await GetVehicles();

            var vehiclesEnumerable = vehicles.ToArray();

            foreach (var v in vehiclesEnumerable)
            {
                if (!string.IsNullOrEmpty(v.CloudId)) continue;

                try
                {
                    var data = new StringContent(JsonConvert.SerializeObject(v), Encoding.UTF8, "application/json");

                    var response = await RestService.Client.PostAsync("/api/Vehicles/", data);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();

                        var em = errorMessage;

                        continue;
                    }

                    var result =
                        JsonConvert.DeserializeObject<VehicleViewModel>(await response.Content.ReadAsStringAsync());

                    if (result == null) continue;

                    using (var transaction = DatabaseService.Realm.BeginWrite())
                    {
                        var vehicle = await Host.VehicleService.GetVehicle(v.Id);

                        vehicle.CloudId = result.Id;

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
        }
    }
}