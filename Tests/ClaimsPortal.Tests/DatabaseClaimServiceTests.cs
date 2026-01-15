using System;
using System.Linq;
using System.Threading.Tasks;
using ClaimsPortal.Data;
using ClaimsPortal.Models;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClaimsPortal.Tests
{
    [TestClass]
    public class DatabaseClaimServiceTests
    {
        private ClaimsPortalDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ClaimsPortalDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ClaimsPortalDbContext(options);
        }

        [TestMethod]
        public async Task SaveDraft_PolicyExpiry_IsPersisted()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateInMemoryContext(dbName);
            var svc = new DatabaseClaimService(ctx);

            var expiry = new DateTime(2026, 12, 31);

            var claim = new Claim
            {
                PolicyInfo = new Policy { PolicyNumber = "P123", ExpiryDate = expiry }
            };

            var request = new FnolSaveRequest { ClaimData = claim, CreatedBy = "test" };

            var saved = await svc.SaveFnolDraftWithDetailsAsync(request);

            var fnol = ctx.FNOLs.FirstOrDefault(f => f.FnolId == saved.FnolId);
            Assert.IsNotNull(fnol);
            Assert.AreEqual(expiry.Date, fnol.PolicyExpirationDate?.Date);
        }

        [TestMethod]
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
            Assert.IsNotNull(fnol);
            Assert.IsTrue(fnol.ReportedByEntityId.HasValue);

            var entity = await ctx.EntityMasters.FindAsync(fnol.ReportedByEntityId.Value);
            Assert.IsNotNull(entity);
            Assert.AreEqual("Jane Reporter", entity.EntityName);

            var addr = ctx.Addresses.FirstOrDefault(a => a.EntityId == entity.EntityId);
            Assert.IsNotNull(addr);
            Assert.AreEqual("123 Main St", addr.StreetAddress);
            Assert.AreEqual("Townsville", addr.City);
        }
    }
}
