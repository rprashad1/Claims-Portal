using System;
using System.Threading.Tasks;

using Data = ClaimsPortal.Data;
using Models = ClaimsPortal.Models;

namespace ClaimsPortal.Services
{
    /// <summary>
    /// Responsible for creating EntityMaster and related AddressMaster records
    /// for parties entered in the UI (e.g. ReportedBy when 'Other').
    /// </summary>
    public class EntityCreationService
    {
        private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<Data.ClaimsPortalDbContext> _dbFactory;

        public EntityCreationService(Microsoft.EntityFrameworkCore.IDbContextFactory<Data.ClaimsPortalDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<long> CreateReportedByEntityAsync(Models.ClaimLossDetails lossDetails, string createdBy)
        {
            if (lossDetails == null) throw new ArgumentNullException(nameof(lossDetails));

            var entity = new Data.EntityMaster
            {
                EntityType = 'I',
                PartyType = "ReportedBy",
                EntityGroupCode = "Claimant",
                EntityName = lossDetails.ReportedByName,
                HomeBusinessPhone = lossDetails.ReportedByPhone,
                Email = lossDetails.ReportedByEmail,
                FEINorSS = lossDetails.ReportedByFeinSsNumber,
                EntityStatus = 'Y',
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy
            };

            using (var ctx = _dbFactory.CreateDbContext())
            {
                ctx.EntityMasters.Add(entity);
                await ctx.SaveChangesAsync();

                if (lossDetails.ReportedByAddress != null && lossDetails.ReportedByAddress.HasAnyAddress)
                {
                    var addr = new Data.AddressMaster
                    {
                        EntityId = entity.EntityId,
                        AddressType = 'M',
                        StreetAddress = lossDetails.ReportedByAddress.StreetAddress,
                        Apt = lossDetails.ReportedByAddress.AddressLine2,
                        City = lossDetails.ReportedByAddress.City,
                        State = lossDetails.ReportedByAddress.State,
                        ZipCode = lossDetails.ReportedByAddress.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    };

                    ctx.Addresses.Add(addr);
                    await ctx.SaveChangesAsync();
                }
            }

            return entity.EntityId;
        }
    }
}
