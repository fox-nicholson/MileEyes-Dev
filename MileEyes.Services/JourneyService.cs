using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Journey;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    internal class JourneyService : IJourneyService
    {
        private readonly List<Journey> _journeys = new List<Journey>();
        public event EventHandler JourneySaved = delegate { };

        public async Task<IEnumerable<Journey>> GetJourneys()
        {
            var journeys = DatabaseService.Realm.All<Journey>();

            return journeys;
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

                // Possibly check for null on Vehicle, if its null return, but see what its like for now, if you keep getting null vehicles when syncing then deal with it here

                journey.Vehicle = vehicle;

                if (j.Company != null)
                {
                    var company = (await Host.CompanyService.GetCompanies()).FirstOrDefault();

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
                            Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Latitude,
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Longitude,
                                0);

                    journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Label = firstAddress.Label;
                }
                else
                {
                    var firstAddress =
                        await
                            Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().PlaceId);

                    journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Label = firstAddress.Label;
                }

                if (string.IsNullOrEmpty(journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().PlaceId))
                {
                    var lastAddress =
                        await
                            Host.GeocodingService.GetAddress(
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Latitude,
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Longitude,
                                journey.Waypoints.Count - 1);

                    journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Label = lastAddress.Label;
                }
                else
                {
                    var lastAddress =
                        await
                            Host.GeocodingService.GetAddress(
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

        public async Task<IEnumerable<Journey>> GetAllJourneys()
        {
            return DatabaseService.Realm.All<Journey>();
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

                var vehicles = await Host.VehicleService.GetVehicles();
                var companies = await Host.CompanyService.GetCompanies();

                foreach (var journeyData in result)
                {
                    var existingJourney = (await GetAllJourneys()).FirstOrDefault(v => v.CloudId == journeyData.Id);

                    var vehicle = vehicles.FirstOrDefault(v => v.CloudId == journeyData.Vehicle.Id);
                    var company = companies.FirstOrDefault(c => c.CloudId == journeyData.Company.Id);

                    if (existingJourney == null)
                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            var journey = DatabaseService.Realm.CreateObject<Journey>();
                            journey.CloudId = journeyData.Id;
                            journey.Date = journeyData.Date;
                            journey.Accepted = journeyData.Accepted;
                            journey.Rejected = journeyData.Rejected;
                            journey.Cost = journeyData.Cost;
                            journey.Distance = journeyData.Distance;
                            journey.Reason = journeyData.Reason;
                            journey.Invoiced = journeyData.Invoiced;
                            journey.Passengers = journeyData.Passengers;
                            journey.Vehicle = vehicle;
                            journey.Company = company;

                            foreach (var waypoint in journeyData.Waypoints)
                            {
                                var newWaypoint = DatabaseService.Realm.CreateObject<Waypoint>();
                                newWaypoint.Latitude = waypoint.Latitude;
                                newWaypoint.Longitude = waypoint.Longitude;
                                newWaypoint.Step = waypoint.Step;
                                newWaypoint.Timestamp = waypoint.Timestamp;
                                newWaypoint.PlaceId = waypoint.PlaceId;

                                journey.Waypoints.Add(newWaypoint);
                            }

                            var first = journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault();

                            first.Label = (await Host.GeocodingService.GetAddress(first.PlaceId)).Label;

                            var last = journey.Waypoints.OrderBy(w => w.Step).LastOrDefault();

                            last.Label = (await Host.GeocodingService.GetAddress(last.PlaceId)).Label;

                            transaction.Commit();
                        }
                    else
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
                            existingJourney.Company = company;

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
            var journeys = await GetJourneys();

            var journeysEnumerable = journeys as Journey[] ?? journeys.ToArray();

            foreach (var j in journeysEnumerable.Where(j => j.MarkedForDeletion == false))
                try
                {
                    if (!string.IsNullOrEmpty(j.CloudId)) continue;

                    if (j.Company == null)
                    {
                        var companies = await Host.CompanyService.GetCompanies();

                        var companiesEnumerable = companies.ToArray();

                        var personalCompany = companiesEnumerable.FirstOrDefault(c => c.Personal);

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