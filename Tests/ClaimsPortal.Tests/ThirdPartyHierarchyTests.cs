using ClaimsPortal.Models;
using System.Linq;
using Xunit;

namespace ClaimsPortal.Tests
{
    public class ThirdPartyHierarchyTests
    {
        [Fact]
        public void InjuredParty_Should_Link_To_Vehicle_By_Vin()
        {
            var vehicle = new ThirdParty
            {
                Name = "Third Vehicle",
                Type = "Vehicle",
                Vehicle = new VehicleInfo { Vin = "VIN123" }
            };

            var passenger = new ThirdParty
            {
                Name = "Passenger One",
                Type = "Passenger",
                SelectedVehicleVin = "VIN123",
                WasInjured = true
            };

            var list = new System.Collections.Generic.List<ThirdParty> { vehicle, passenger };

            var linked = list.Where(p => p.Type != "Vehicle" && !string.IsNullOrEmpty(p.SelectedVehicleVin) && p.SelectedVehicleVin == (vehicle.Vehicle?.Vin ?? string.Empty)).ToList();

            Assert.Single(linked);
            Assert.Equal("Passenger One", linked[0].Name);
        }
    }
}
