using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Journey;
using MileEyes.PublicModels.Vehicles;
using MileEyes.Services.Models;
using Newtonsoft.Json;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace MileEyes.Services
{
    class JourneyService : IJourneyService
    {
        public event EventHandler JourneySaved = delegate { };

        private List<Journey> _journeys = new List<Journey>();

        public async Task<IEnumerable<Journey>> GetJourneys()
        {
            var journeys = DatabaseService.Realm.All<Journey>();

            return journeys;
        }

        public async Task<IEnumerable<Journey>> GetAllJourneys()
        {
            return DatabaseService.Realm.All<Journey>();
        }

        public async Task<Journey> GetJourney(string id)
        {
            return DatabaseService.Realm.All<Journey>().FirstOrDefault(j => j.Id == id);
        }

        public async Task<Journey> SaveJourney(Journey j)
        {
            j.Id = Guid.NewGuid().ToString();

            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var journey = DatabaseService.Realm.CreateObject<Journey>();

                journey.Id = j.Id;

                journey.CloudId = j.CloudId;
                journey.Date = j.Date;
                journey.Accepted = j.Accepted;
                journey.Rejected = j.Rejected;
                journey.Cost = j.Cost;
                journey.Distance = j.Distance;
                journey.Reason = j.Reason;
                journey.Invoiced = j.Invoiced;
                journey.Passengers = j.Passengers;

                var vehicle = await Host.VehicleService.GetVehicle(j.Vehicle.Id);

                journey.Vehicle = vehicle;

                if (j.Company != null)
                {
                    var company = (await Services.Host.CompanyService.GetCompanies()).FirstOrDefault();

                    journey.Company = company;
                }

                foreach (var w in j.Waypoints)
                {
                    var waypoint = DatabaseService.Realm.CreateObject<Waypoint>();
                    waypoint.Step = w.Step;
                    waypoint.Latitude = w.Latitude;
                    waypoint.Longitude = w.Longitude;
                    waypoint.Timestamp = w.Timestamp;
                    waypoint.PlaceId = w.PlaceId;
                    waypoint.Id = w.Id;

                    journey.Waypoints.Add(waypoint);
                }

                if (string.IsNullOrEmpty(journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().PlaceId))
                {
                    var firstAddress =
                        await
                            Services.Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Latitude,
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Longitude);

                    journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Label = firstAddress.Label;
                }
                else
                {
                    var firstAddress =
                        await
                            Services.Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().PlaceId);

                    journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Label = firstAddress.Label;
                }

                if (string.IsNullOrEmpty(journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().PlaceId))
                {
                    var lastAddress =
                        await
                            Services.Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Latitude,
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Longitude);

                    journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Label = lastAddress.Label;
                }
                else
                {
                    var lastAddress =
                        await
                            Services.Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().PlaceId);

                    journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Label = lastAddress.Label;
                }

                transaction.Commit();

                JourneySaved?.Invoke(this, EventArgs.Empty);

                return journey;
            }
        }

        public async Task<Journey> DeleteJourney(string id)
        {
            var j = _journeys.Find(journey => journey.Id == id);

            if (j == null) return null;

            _journeys.Remove(j);

            return j;
        }

        public async Task Sync()
        {
            await PushNew();
            await PullUpdate();
        }
        private async Task PullUpdate()
        {
            try
            {
                var response = await RestService.Client.GetAsync("/api/Journeys/");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    var em = errorMessage;

                    return;
                }

                var responseText = await response.Content.ReadAsStringAsync();

                var result =
                    JsonConvert.DeserializeObject<ICollection<JourneyViewModel>>(responseText);

                foreach (var journeyData in result)
                {
                    var existingJourney = (await GetAllJourneys()).FirstOrDefault(v => v.CloudId == journeyData.Id);

                    if (existingJourney == null)
                    {
                        var vehicles = await Host.VehicleService.GetVehicles();
                        var companies = await Host.CompanyService.GetCompanies();

                        var vehicle = vehicles.FirstOrDefault(v => v.CloudId == journeyData.Vehicle.Id);
                        var company = companies.FirstOrDefault(c => c.CloudId == journeyData.Company.Id);

                        var journey = new Journey
                        {
                            CloudId = journeyData.Id,
                            Date = journeyData.Date,
                            Accepted = journeyData.Accepted,
                            Rejected = journeyData.Rejected,
                            Cost = journeyData.Cost,
                            Distance = journeyData.Distance,
                            Reason = journeyData.Reason,
                            Invoiced = journeyData.Invoiced,
                            Passengers = journeyData.Passengers,
                            Vehicle = vehicle,
                            Company = company
                        };

                        foreach (var waypoint in journeyData.Waypoints)
                        {
                            journey.Waypoints.Add(new Waypoint()
                            {
                                Latitude = waypoint.Latitude,
                                Longitude = waypoint.Longitude,
                                Step = waypoint.Step,
                                Timestamp = waypoint.Timestamp,
                                PlaceId = waypoint.PlaceId
                            });
                        }

                        await SaveJourney(journey);
                    }
                    else
                    {
                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            existingJourney.Date = journeyData.Date;
                            existingJourney.Accepted = journeyData.Accepted;
                            existingJourney.Rejected = journeyData.Rejected;
                            existingJourney.Cost = journeyData.Cost;
                            existingJourney.Distance = journeyData.Distance;
                            existingJourney.Reason = journeyData.Reason;
                            existingJourney.Invoiced = journeyData.Invoiced;
                            existingJourney.Passengers = journeyData.Passengers;

                            var vehicle = await Host.VehicleService.GetVehicle(journeyData.Vehicle.Id);

                            existingJourney.Vehicle = vehicle;

                            if (journeyData.Company != null)
                            {
                                var company = (await Host.CompanyService.GetCompanies()).FirstOrDefault();

                                existingJourney.Company = company;
                            }

                            transaction.Commit();
                        }
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
            var journeys = await GetJourneys();

            var journeysEnumerable = journeys as Journey[] ?? journeys.ToArray();

            foreach (var j in journeysEnumerable.Where(j => j.MarkedForDeletion == false))
            {
                try
                {
                    if (!string.IsNullOrEmpty(j.CloudId)) continue;

                    if (j.Company == null)
                    {
                        var companies = await Host.CompanyService.GetCompanies();

                        var companiesEnumerable = companies.ToArray();

                        var personalCompany = companiesEnumerable.FirstOrDefault(c => c.Personal == true);

                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            j.Company = personalCompany;

                            transaction.Commit();
                        }
                    }

                    var data = new StringContent(JsonConvert.SerializeObject(j), Encoding.UTF8, "application/json");

                    var response = await RestService.Client.PostAsync("/api/Journeys/", data);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();

                        var em = errorMessage;

                        continue;
                    }

                    var result =
                        JsonConvert.DeserializeObject<JourneyViewModel>(await response.Content.ReadAsStringAsync());

                    if (result == null) continue;

                    using (var transaction = DatabaseService.Realm.BeginWrite())
                    {
                        j.CloudId = result.Id;
                        j.Cost = result.Cost;
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