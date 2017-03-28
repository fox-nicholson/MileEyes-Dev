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
        public static bool VehicleSyncing;
        public async Task<Vehicle> AddVehicle(Vehicle v)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var vehicle = DatabaseService.Realm.CreateObject<Vehicle>();

                var engineType =
                    DatabaseService.Realm.All<EngineType>().FirstOrDefault(et => et.Id == v.EngineType.Id);

                var vehicleType =
                    DatabaseService.Realm.All<VehicleType>().FirstOrDefault(et => et.Id == v.VehicleType.Id);

                vehicle.Id = Guid.NewGuid().ToString();
                vehicle.CloudId = v.CloudId;
                vehicle.Registration = v.Registration.ToUpper();
                vehicle.EngineType = engineType;
                vehicle.VehicleType = vehicleType;
                vehicle.RegDate = v.RegDate;

                transaction.Commit();
                transaction.Dispose();

                await PushNew();

                return vehicle;
            }
        }

        public Vehicle GetVehicle(string id)
        {
            return DatabaseService.Realm.Find<Vehicle>(id);
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
                var vehicle = GetVehicle(id);

                var journeys = (await Host.JourneyService.GetJourneys()).ToArray();
                var vehiclesJourneys = journeys.Where(j => j.Vehicle.Id == vehicle.Id);

                if (vehiclesJourneys.Any()) return null;

                if (!string.IsNullOrEmpty(vehicle.CloudId))
                {
                    vehicle.MarkedForDeletion = true;
                }
                else
                {
                    DatabaseService.Realm.Remove(vehicle);
                }

                transaction.Commit();
                transaction.Dispose();

                return vehicle;
            }
        }

        public async Task SetDefault(string id)
        {
            if (!TrackerService.IsTracking && !JourneyService.JourneySyncing && !VehicleTypeService.VehicleTypeSyncing && !CompanyService.CompanySyncing && !EngineTypeService.EngineTypeSyncing)
            {
                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    var vehicles = DatabaseService.Realm.All<Vehicle>();

                    if (!(vehicles.Where(d => d.Default).Any()) && vehicles.Any())
                    {
                        vehicles.FirstOrDefault().Default = true;
                        transaction.Commit();
                        transaction.Dispose();
                        return;
                    }

                    foreach (var vehicle in vehicles)
                    {
                        vehicle.Default = vehicle.Id == id;
                    }

                    transaction.Commit();
                    transaction.Dispose();
                }
            }
        }

        public async Task Sync()
        {
            if (!TrackerService.IsTracking && !JourneyService.JourneySyncing && !VehicleTypeService.VehicleTypeSyncing && !CompanyService.CompanySyncing && !EngineTypeService.EngineTypeSyncing)
            {
                VehicleSyncing = true;
                await PushNew();
                await PullUpdate();
                await DeleteMarked();
                VehicleSyncing = false;
            }
        }

        private async Task DeleteMarked()
        {
            var vehicles = await GetAllVehicles();

            foreach (var v in vehicles.Where(v => v.MarkedForDeletion))
            {
                try
                {
                    RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                    var response = await RestService.Client.DeleteAsync("/api/Vehicles/" + v.CloudId);

                    if (!response.IsSuccessStatusCode) continue;

                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            var vehicleToRemove = GetVehicle(v.Id);
                            DatabaseService.Realm.Remove(vehicleToRemove);

                            transaction.Commit();
                            transaction.Dispose();
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
                RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                var response = await RestService.Client.GetAsync("/api/Vehicles/");

                if (response == null) return;

                if (!response.IsSuccessStatusCode) return;

                var result =
                    JsonConvert.DeserializeObject<ICollection<VehicleViewModel>>(
                        await response.Content.ReadAsStringAsync());

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

                        var vehicleTypes = await Host.VehicleTypeService.GetVehicleTypes();

                        var vehicleType = vehicleTypes.FirstOrDefault(vt => vt.Id == vehicleData.VehicleType.Id);

                        vehicle.CloudId = vehicleData.Id;
                        vehicle.Registration = vehicleData.Registration;
                        vehicle.EngineType = engineType;
                        vehicle.VehicleType = vehicleType;
                        vehicle.RegDate = vehicleData.RegDate;

                        await AddVehicle(vehicle);

                        return;
                    }

                    var existingVehicle = GetVehicle(existingVehiclesEnumerable.FirstOrDefault().Id);

                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            var engineTypes = await Host.EngineTypeService.GetEngineTypes();

                            var engineType = engineTypes.FirstOrDefault(
                                et => et.Id == vehicleData.EngineType.Id);

                            var a = engineType;

                            existingVehicle.EngineType = engineType;

                            var vehicleTypes = await Host.VehicleTypeService.GetVehicleTypes();

                            var vehicleType = vehicleTypes.FirstOrDefault(
                                vt => vt.Id == vehicleData.VehicleType.Id);

                            var t = vehicleType;

                            existingVehicle.VehicleType = vehicleType;

                            existingVehicle.Registration = vehicleData.Registration;

                            transaction.Commit();
                            transaction.Dispose();
                        }
                }
            } catch (Exception)
            {
                return;
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
                    RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                    var data = new StringContent(JsonConvert.SerializeObject(v), Encoding.UTF8, "application/json");

                    var response = await RestService.Client.PostAsync("/api/Vehicles/", data);

                    if (response == null) return;

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
                            var vehicle = Host.VehicleService.GetVehicle(v.Id);

                            vehicle.CloudId = result.Id;

                            transaction.Commit();
                            transaction.Dispose();
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