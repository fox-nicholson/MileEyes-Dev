using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;
using Plugin.Geolocator.Abstractions;

namespace MileEyes.Services
{
    public interface ITrackerService
    {
        Position CurrentLocation { get; }
        double CurrentDistance { get; }

        IEnumerable<Waypoint> CurrentWaypoints { get; }
        
        event EventHandler HasMoved;
        event EventHandler Started;
        event EventHandler Cancelled;
        event EventHandler<Journey> Stopped;
        event EventHandler<string> StartFailed;

        Task Reset();
        Task Start();
        Task Stop();
        Task Cancel();
    }
}
