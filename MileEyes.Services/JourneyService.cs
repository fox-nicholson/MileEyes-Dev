using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Journey;
using MileEyes.Services.Models;
using Newtonsoft.Json;
using Xamarin.Forms;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Vehicles;
using MileEyes.PublicModels.EngineTypes;
using MileEyes.PublicModels.VehicleTypes;

namespace MileEyes.Services
{
    internal class JourneyService : IJourneyService
    {
        private readonly List<Journey> _journeys = new List<Journey>();
        public event EventHandler JourneySaved = delegate { };
        private bool Busy;

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

                if (j.Company == null)
                {
                    var company = (await Host.CompanyService.GetCompanies()).FirstOrDefault();

                    journey.Company = company;
                }
                else if (j.Company.Name == "N/A")
                {
                    journey.Company = null;
                }
                else
                {
                    journey.Company = j.Company;
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
                                journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault().Longitude);

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
                                journey.Waypoints.OrderBy(w => w.Step).LastOrDefault().Longitude);

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
                transaction.Dispose();

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
            if (!TrackerService.IsTracking)
            {
                await PushNew();
                await PullUpdate();
            }
        }

        public async Task<IEnumerable<Journey>> GetAllJourneys()
        {
            return DatabaseService.Realm.All<Journey>();
        }

        private async Task PullUpdate()
        {
            if (Busy) return;
            Busy = true;
            var requestedWeek = new DateTime();
            var startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
            var weeksInYearTs = (DateTime.Today - startOfYear).TotalDays / 7;
            var weeksInYear = (int) Math.Ceiling(weeksInYearTs);
            var monday = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek + (int) DayOfWeek.Monday);

            for (var i = 0; i < weeksInYear; i++)
            {
                requestedWeek = new DateTime(monday.Year, monday.Month, monday.Day).AddDays(-7 * i);
                var requestedDay = requestedWeek.Day;
                var requestedMonth = requestedWeek.Month;
                var response = await RestService.Client.GetAsync("/api/Journeys/" + requestedDay + "/" + requestedMonth);

                if (response == null)
                {
                    Busy = false;
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    var em = errorMessage;

                    Busy = false;

                    return;
                }

                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<ICollection<JourneyViewModel>>(responseText);

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

                            var startPoint = 0;
                            var endPoint = journeyData.Waypoints;
                            var finish = true;

                            for (int w = 0; finish; w++)
                            {
                                startPoint = w * 50;

                                finish = true;

                                var waypointResponse = await RestService.Client.GetAsync("/api/Waypoints/" + journey.CloudId + "/" + startPoint);

                                if (waypointResponse == null)
                                {
                                    Busy = false;
                                    return;
                                }

                                if (!waypointResponse.IsSuccessStatusCode)
                                {
                                    var errorMessage = await waypointResponse.Content.ReadAsStringAsync();

                                    var em = errorMessage;

                                    Busy = false;

                                    return;
                                }

                                var waypointResponseText = await waypointResponse.Content.ReadAsStringAsync();

                                var waypointResult = JsonConvert.DeserializeObject<WaypointsViewModel>(waypointResponseText);

                                if (waypointResult.Waypoints.Count == 2) endPoint = 1;

                                foreach (var newWaypoint in waypointResult.Waypoints)
                                {
                                    var waypointToAdd = new Waypoint()
                                    {
                                        Id = newWaypoint.Id,
                                        Latitude = newWaypoint.Latitude,
                                        Longitude = newWaypoint.Longitude,
                                        PlaceId = newWaypoint.PlaceId,
                                        Step = newWaypoint.Step,
                                        Timestamp = newWaypoint.Timestamp,
                                        Label = ""
                                    };
                                    journey.Waypoints.Add(waypointToAdd);

                                    if (journey.Waypoints.Count == endPoint) finish = false;
                                    if (endPoint == 1) finish = false;
                                }
                            }

                            var first = journey.Waypoints.OrderBy(w => w.Step).FirstOrDefault();

                            first.Label = (await Host.GeocodingService.GetAddress(first.PlaceId)).Label;

                            var last = journey.Waypoints.OrderBy(w => w.Step).LastOrDefault();

                            last.Label = (await Host.GeocodingService.GetAddress(last.PlaceId)).Label;

                            transaction.Commit();
                            transaction.Dispose();
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
                            transaction.Dispose();
                        }
                }
            }
            Busy = false;
        }

        private bool _busy;

        private async Task PushNew()
        {
            if (Busy) return;
            Busy = true;

            var journeys = await GetJourneys();

            var journeysEnumerable = journeys as Journey
                                            []
                                        ?? journeys.ToArray();

            foreach (var j in journeysEnumerable.Where(j => j.MarkedForDeletion == false))
            {
                //try
                //{
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

                    var waypoints = j.Waypoints;


                    var journey = new JourneyBindingModel()
                    {
                        Company = new CompanyBindingModel()
                        {
                            CloudId = j.Company.CloudId,
                            Id = j.Company.Id,
                            Name = j.Company.Name,
                            Personal = j.Company.Personal
                        },
                        Date = j.Date,
                        Distance = j.Distance,
                        Invoiced = j.Invoiced,
                        Passengers = j.Passengers,
                        Reason = j.Reason,
                        Vehicle = new VehicleBindingModel()
                        {
                            CloudId = j.Vehicle.CloudId,
                            Id = j.Vehicle.Id,
                            RegDate = j.Vehicle.RegDate,
                            Registration = j.Vehicle.Registration,
                            EngineType = new EngineTypeBindingModel()
                            {
                                Id = j.Vehicle.EngineType.Id
                            },
                            VehicleType = new VehicleTypeBindingModel()
                            {
                                Id = j.Vehicle.VehicleType.Id
                            }
                        },
                        Waypoints = waypoints.Count - 1
                    };

                    var data = new StringContent(JsonConvert.SerializeObject(journey), Encoding.UTF8, "application/json");

                    var response = await RestService.Client.PostAsync("/api/Journeys/", data);

                    if (response == null)
                    {
                        Busy = false;
                        return;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();

                        var em = errorMessage;

                        continue;
                    }

                    var result =
                        JsonConvert.DeserializeObject<JourneyViewModel>(
                            await response.Content.ReadAsStringAsync());

                    if (result == null) continue;

                var waypointsModel = new WaypointsBindingModel()
                {
                    first = waypoints.First().Step,
                    last = waypoints.Last().Step,
                    Journey = journey,
                        journeyId = result.Id.ToString(),
                        Waypoints = new List<WaypointBindingModel>()
                    };

                    if (waypoints.Count > 0)
                    {
                        var finish = true;
                        for (int i = 0; finish; i++)
                        {

                            var startPoint = i * 50;
                            var endPoint = startPoint + 49;

                            if (endPoint > waypoints.Count) endPoint = waypoints.Count;

                            for (int w = startPoint; w < endPoint; w++)
                            {
                                waypointsModel.Waypoints.Add(new WaypointBindingModel()
                                {
                                    Latitude = waypoints.ElementAt(w).Latitude,
                                    Longitude = waypoints.ElementAt(w).Longitude,
                                    PlaceId = waypoints.ElementAt(w).PlaceId,
                                    Step = waypoints.ElementAt(w).Step,
                                    Timestamp = waypoints.ElementAt(w).Timestamp
                                });
                            }
                            
                            data = new StringContent(JsonConvert.SerializeObject(waypointsModel), Encoding.UTF8, "application/json");

                            response = await RestService.Client.PostAsync("/api/Waypoints/", data);

                            if (response == null)
                            {
                                Busy = false;
                                return;
                            }

                            if (!response.IsSuccessStatusCode)
                            {
                                var errorMessage = await response.Content.ReadAsStringAsync();

                                var em = errorMessage;

                                continue;
                            }

                            var waypointResult =
                                JsonConvert.DeserializeObject<WaypointsViewModel>(
                                    await response.Content.ReadAsStringAsync());

                            if (result == null) continue;

                            if (endPoint == waypoints.Count) finish = false;
                        }
                    }

                    using (var transaction = DatabaseService.Realm.BeginWrite())
                    {
                        j.CloudId = result.Id;
                        transaction.Commit();
                        transaction.Dispose();
                    }
                //}
                //catch (Exception ex)

                //{
                //    var message = ex.Message;
                //}
                Busy = false;
            }
            Busy = false;
        }

        private bool Wait()
        {
            Busy = false;
            return false;
        }
    }
}