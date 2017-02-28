using System;

namespace MileEyes.Services.Models
{
    public class SharedJourney
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset Date { get; set; }
        public double Distance { get; set; }
        public bool Invoiced { get; set; }
        public int Passengers { get; set; }
        public string Vehicle { get; set; }
    }
}