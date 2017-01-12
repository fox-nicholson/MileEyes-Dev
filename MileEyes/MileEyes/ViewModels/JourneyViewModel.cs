using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Extensions;
using MileEyes.Services.Helpers;
using MileEyes.Services.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.ViewModels
{
    public class JourneyViewModel : ViewModel
    {
        private DateTimeOffset _date;

        public DateTimeOffset Date
        {
            get { return _date; }
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private Address _originAddress;

        public Address OriginAddress
        {
            get { return _originAddress; }
            set
            {
                if (_originAddress == value) return;
                _originAddress = value;
                OnPropertyChanged(nameof(OriginAddress));
            }
        }

        private Address _destinationAddress;

        public Address DestinationAddress
        {
            get { return _destinationAddress; }
            set
            {
                if (_destinationAddress == value) return;
                _destinationAddress = value;
                OnPropertyChanged(nameof(DestinationAddress));
            }
        }

        private Vehicle _vehicle;

        public Vehicle Vehicle
        {
            get { return _vehicle; }
            set
            {
                if (_vehicle == value) return;
                _vehicle = value;
                OnPropertyChanged(nameof(Vehicle));
            }
        }

        private Company _company;

        public Company Company
        {
            get { return _company; }
            set
            {
                if (_company == value) return;
                _company = value;
                OnPropertyChanged(nameof(Company));
            }
        }

        private bool _invoiced;

        public bool Invoiced
        {
            get { return _invoiced; }
            set
            {
                if (_invoiced == value) return;
                _invoiced = value;
                OnPropertyChanged(nameof(Invoiced));
            }
        }

        private string _reason;

        public string Reason
        {
            get { return _reason; }
            set
            {
                if (_reason == value) return;
                _reason = value;
                OnPropertyChanged(nameof(Reason));
            }
        }

        private int _passengers;

        public int Passengers
        {
            get { return _passengers; }
            set
            {
                if (_passengers == value) return;
                _passengers = value;
                OnPropertyChanged(nameof(Passengers));
            }
        }

        private double _distance;

        public double Distance
        {
            get { return _distance; }
            set
            {
                if (_distance == value) return;
                _distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }

        private decimal _cost;

        public decimal Cost
        {
            get { return _cost; }
            set
            {
                if (_cost == value) return;
                _cost = value;
                OnPropertyChanged(nameof(Cost));
            }
        }

        private bool _hasCost;

        public bool HasCost
        {
            get { return _hasCost; }
            set
            {
                if (_hasCost == value) return;
                _hasCost = value;
                OnPropertyChanged(nameof(HasCost));
                OnPropertyChanged(nameof(NoCost));
            }
        }

        public bool NoCost => !_hasCost;

        public bool Accepted { get; set; } = false;

        public bool Manual { get; set; }
        public bool Gps { get; set; }

        public ObservableCollection<Position> Route { get; set; } = new ObservableCollection<Position>();

        public ObservableCollection<Waypoint> Waypoints { get; set; } = new ObservableCollection<Waypoint>();

        public JourneyViewModel()
        {

        }

        public JourneyViewModel(Journey j)
        {
            Init(j);
        }

        private async void Init(Journey j)
        {
            ShareCommand = new Command(Share);

            Date = j.Date;
            
            Distance = Units.MetersToMiles(await j.CalculateDistance());

            Cost = Convert.ToDecimal(j.Cost);

            Waypoints.Clear();

            foreach (var w in j.Waypoints)
            {
                Waypoints.Add(w);
            }

            var originWaypoint = Waypoints.OrderBy(w => w.Step).First();
            var destinationWaypoint = Waypoints.OrderBy(w => w.Step).Last();

            OriginAddress = new Address()
            {
                Label = originWaypoint.Label,
                Latitude = originWaypoint.Latitude,
                Longitude = originWaypoint.Longitude,
                PlaceId = originWaypoint.PlaceId
            };

            DestinationAddress = new Address()
            {
                Label = destinationWaypoint.Label,
                Latitude = destinationWaypoint.Latitude,
                Longitude = destinationWaypoint.Longitude,
                PlaceId = destinationWaypoint.PlaceId
            };

            if (Waypoints.Count() > 2)
            {
                Gps = true;
                OnPropertyChanged(nameof(Gps));
            }
            else
            {
                Manual = true;
                OnPropertyChanged(nameof(Manual));
            }

            Invoiced = j.Invoiced;
            Passengers = j.Passengers;
            Vehicle = j.Vehicle;
            Company = j.Company;
            Reason = j.Reason;
        }

        public async void InitRoute()
        {
            if (Waypoints.Count() > 2)
            {
                Route.Clear();

                foreach (var w in Waypoints)
                {
                    Route.Add(new Position(w.Latitude, w.Longitude));
                }
            }
            else
            {
                var leg = await Services.Host.GeocodingService.GetDirectionsFromGoogle(new[] { OriginAddress.Latitude, OriginAddress.Longitude },
                    new[] { DestinationAddress.Latitude, DestinationAddress.Longitude });


                Route.Clear();

                foreach (var step in leg.steps)
                {
                    Route.Add(new Position(step.end_location.lat, step.end_location.lng));
                }
            }
        }

        public ICommand ShareCommand { get; set; }

        public async void Share()
        {
            var sharedJourney = new SharedJourney()
            {
                From = OriginAddress.Label,
                To = DestinationAddress.Label,
                Reason = Reason,
                Date = Date,
                Distance = Distance,
                Invoiced = Invoiced,
                Passengers = Passengers,
                Vehicle = Vehicle.Registration
            };

            MessagingCenter.Send<SharedJourney>(sharedJourney, "Share");
        }
    }
}
