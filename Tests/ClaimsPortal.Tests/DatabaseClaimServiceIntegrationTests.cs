using ClaimsPortal.Data;
using ClaimsPortal.Models;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace ClaimsPortal.Tests
{
    public class DatabaseClaimServiceIntegrationTests
    {
        [Fact]
        public async Task SaveFnolDraftWithDetails_Persists_VehicleAndPassengerVin()
        {
            var options = new DbContextOptionsBuilder<ClaimsPortalDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SaveFnol")
                .Options;

            using var context = new ClaimsPortalDbContext(options);
            var service = new DatabaseClaimService(context);

            // Prepare claim with a vehicle and a passenger linked by VIN
            var vehicle = new ThirdParty
            {
                Name = "Other Vehicle",
                Type = "Vehicle",
                Vehicle = new VehicleInfo { Vin = "VIN-TEST-123", Make = "MakeA", Model = "ModelX", Year = 2020 }
            };

            var passenger = new ThirdParty
            {
                Name = "Passenger A",
                Type = "Passenger",
                SelectedVehicleVin = "VIN-TEST-123",
                WasInjured = true,
                InjuryInfo = new Injury { InjuryDescription = "Minor", InjuryType = "Whiplash" }
            };

            var claim = new Claim
            {
                ClaimNumber = "TST-1",
                CreatedDate = System.DateTime.Now,
                ThirdParties = new System.Collections.Generic.List<ThirdParty> { vehicle, passenger }
            };

            var request = new FnolSaveRequest
            {
                ClaimData = claim,
                CreatedBy = "unit-test"
            };

            var fnol = await service.SaveFnolDraftWithDetailsAsync(request);

            // Assert that vehicle record was created
            var savedVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VIN == "VIN-TEST-123");
            Assert.NotNull(savedVehicle);

            // Assert that a claimant for the passenger was created with the Vin property
            var claimant = await context.Claimants.FirstOrDefaultAsync(c => c.Vin == "VIN-TEST-123");
            Assert.NotNull(claimant);
        }
    }
}
