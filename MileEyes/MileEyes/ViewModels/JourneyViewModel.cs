﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.Services;
using MileEyes.Services.Helpers;
using MileEyes.Services.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.ViewModels
{
    public class JourneyViewModel : ViewModel
    {
        private Company _company;

        private decimal _cost;
        private DateTimeOffset _date;

        private Address _destinationAddress;

        private double _distance;

        private bool _hasCost;

        private bool _invoiced;

        private bool _loaded;

        private Address _originAddress;

        private int _passengers;

        private string _reason;

        private bool _showDetails;

        private Vehicle _vehicle;

        public JourneyViewModel()
        {
        }

        public JourneyViewModel(Journey j)
        {
            Distance = Units.MetersToMiles(j.Distance);
            Cost = Convert.ToDecimal(j.Cost);
            Date = j.Date;

            Waypoints.Clear();

            foreach (var w in j.Waypoints)
                Waypoints.Add(w);

            var originWaypoint = Waypoints.OrderBy(w => w.Step).First();
            var destinationWaypoint = Waypoints.OrderBy(w => w.Step).Last();

            OriginAddress = new Address
            {
                Label = originWaypoint.Label,
                Latitude = originWaypoint.Latitude,
                Longitude = originWaypoint.Longitude,
                PlaceId = originWaypoint.PlaceId
            };

            DestinationAddress = new Address
            {
                Label = destinationWaypoint.Label,
                Latitude = destinationWaypoint.Latitude,
                Longitude = destinationWaypoint.Longitude,
                PlaceId = destinationWaypoint.PlaceId
            };

            if (Waypoints.Count() > 2)
                Gps = true;
            else
                Manual = true;

            Invoiced = j.Invoiced;
            Passengers = j.Passengers;
            Vehicle = j.Vehicle;
            Company = j.Company;
            Reason = j.Reason;

            ShareCommand = new Command(Share);
        }

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

        public bool ShowDetails
        {
            get { return _showDetails; }
            set
            {
                if (_showDetails == value) return;
                _showDetails = value;
                OnPropertyChanged(nameof(ShowDetails));
                OnPropertyChanged(nameof(ShowMap));
            }
        }

        public bool ShowMap => !_showDetails;

        public bool Accepted { get; set; } = false;

        public bool Manual { get; set; }
        public bool Gps { get; set; }

        public ObservableCollection<Position> Route { get; set; } = new ObservableCollection<Position>();

        public ObservableCollection<Waypoint> Waypoints { get; set; } = new ObservableCollection<Waypoint>();

        public bool Loaded
        {
            get { return _loaded; }
            set
            {
                if (_loaded == value) return;
                _loaded = value;
                OnPropertyChanged(nameof(Loaded));
            }
        }

        public ICommand ShareCommand { get; set; }

        public async void InitRoute()
        {
            ShowDetails = true;
            if (Waypoints.Count() > 2)
            {
                Route.Clear();

                foreach (var w in Waypoints)
                    Route.Add(new Position(w.Latitude, w.Longitude));
            }
            else
            {
                var leg =
                    await Host.GeocodingService.GetDirectionsFromGoogle(
                        new[] {OriginAddress.Latitude, OriginAddress.Longitude},
                        new[] {DestinationAddress.Latitude, DestinationAddress.Longitude});

                Route.Clear();
                if (leg != null)
                    foreach (var step in leg.steps)
                        Route.Add(new Position(step.end_location.lat, step.end_location.lng));
            }

            Loaded = true;
        }

        public void Share()
        {
            var from = OriginAddress.Label;
            var to = DestinationAddress.Label;
            var reason = Reason;
            var date = Date;
            var distance = Distance;
            var invoiced = Invoiced;
            var passengers = Passengers;
            var vehicle = Vehicle;

            var sharedJourney = new SharedJourney
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

            MessagingCenter.Send(sharedJourney, "Share");
        }
    }
}