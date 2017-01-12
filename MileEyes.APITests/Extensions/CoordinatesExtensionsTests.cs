using Microsoft.VisualStudio.TestTools.UnitTesting;
using MileEyes.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions.Tests
{
    [TestClass()]
    public class CoordinatesExtensionsTests
    {
        [TestMethod()]
        public void DistanceToTest()
        {
            var miles = 7.30D;
            var km = 11.76D;
            var nmiles = 6.34D;

            var start = new Coordinates() { Latitude = 55.087278, Longitude = -1.611036 };
            var end = new Coordinates() { Latitude = 54.981552, Longitude = -1.609659 };

            var milesDistance = start.DistanceTo(end, UnitOfLength.Miles);
            var kmDistance = start.DistanceTo(end, UnitOfLength.Kilometers);
            var nmilesDistance = start.DistanceTo(end, UnitOfLength.NauticalMiles);

            Assert.AreEqual(miles, Math.Round(milesDistance, 2, MidpointRounding.AwayFromZero));
            Assert.AreEqual(km, Math.Round(kmDistance, 2, MidpointRounding.AwayFromZero));
            Assert.AreEqual(nmiles, Math.Round(nmilesDistance, 2, MidpointRounding.AwayFromZero));
        }
    }
}