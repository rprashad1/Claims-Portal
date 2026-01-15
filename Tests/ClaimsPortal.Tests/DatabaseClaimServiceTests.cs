using System;
using System.Linq;
using System.Threading.Tasks;
using ClaimsPortal.Data;
using ClaimsPortal.Models;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClaimsPortal.Tests
{
    public class DatabaseClaimServiceTests
    {
        private ClaimsPortalDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ClaimsPortalDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ClaimsPortalDbContext(options);
        }

        [Fact]
        public async Task SaveDraft_PolicyExpiry_IsPersisted()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateInMemoryContext(dbName);
            var svc = new DatabaseClaimService(ctx);

            var expiry = new DateTime(2026, 12, 31);

            var claim = new Claim
            {
                PolicyInfo = new ClaimsPortal.Models.Policy { PolicyNumber = "P123", ExpiryDate = expiry }
            };

            var request = new FnolSaveRequest { ClaimData = claim, CreatedBy = "test" };

            var saved = await svc.SaveFnolDraftWithDetailsAsync(request);

            var fnol = ctx.FNOLs.FirstOrDefault(f => f.FnolId == saved.FnolId);
            Assert.NotNull(fnol);
            Assert.Equal(expiry.Date, fnol.PolicyExpirationDate?.Date);
        }

        [Fact]
        public async Task SaveDraft_ReportedByOther_CreatesEntityAndAddress()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateInMemoryContext(dbName);
            var svc = new DatabaseClaimService(ctx);

            var claim = new Claim
            {
                LossDetails = new ClaimLossDetails
                {
                    ReportedBy = "Other",
                    ReportedByName = "Jane Reporter",
                    ReportedByPhone = "555-0100",
                    ReportedByEmail = "jane@example.com",
                    ReportedByAddress = new Address
                    {
                        StreetAddress = "123 Main St",
                        City = "Townsville",
                        State = "TS",
                        ZipCode = "12345"
                    }
                }
            };

            var request = new FnolSaveRequest { ClaimData = claim, CreatedBy = "test" };

            var saved = await svc.SaveFnolDraftWithDetailsAsync(request);

            var fnol = await ctx.FNOLs.FirstOrDefaultAsync(f => f.FnolId == saved.FnolId);
            Assert.NotNull(fnol);
            Assert.True(fnol.ReportedByEntityId.HasValue);

            var entity = await ctx.EntityMasters.FindAsync(fnol.ReportedByEntityId.Value);
            Assert.NotNull(entity);
            Assert.Equal("Jane Reporter", entity.EntityName);

            var addr = ctx.Addresses.FirstOrDefault(a => a.EntityId == entity.EntityId);
            Assert.NotNull(addr);
            Assert.Equal("123 Main St", addr.StreetAddress);
            Assert.Equal("Townsville", addr.City);
        }
    }
}
