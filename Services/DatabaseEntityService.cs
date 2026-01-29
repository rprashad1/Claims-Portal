using ClaimsPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    public interface IDatabaseEntityService
    {
        Task<EntityMaster?> GetEntityAsync(long entityId);
        Task<List<EntityMaster>> GetEntitiesByGroupAsync(string entityGroupCode);
        Task<List<EntityMaster>> GetEntitiesByTypeAsync(string vendorType);
        Task<List<EntityMaster>> GetAdjustersAsync();
        Task<EntityMaster> CreateEntityAsync(EntityMaster entity);
        Task<EntityMaster> UpdateEntityAsync(EntityMaster entity);
        Task<AddressMaster> AddAddressAsync(AddressMaster address);
        Task<List<AddressMaster>> GetEntityAddressesAsync(long entityId);
    }

    public class DatabaseEntityService : IDatabaseEntityService
    {
        private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> _dbFactory;

        public DatabaseEntityService(Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<EntityMaster?> GetEntityAsync(long entityId)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.EntityMasters
                .FirstOrDefaultAsync(e => e.EntityId == entityId && e.EntityStatus == 'Y');
        }

        public async Task<List<EntityMaster>> GetEntitiesByGroupAsync(string entityGroupCode)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.EntityMasters
                .Where(e => e.EntityGroupCode == entityGroupCode && e.EntityStatus == 'Y')
                .OrderBy(e => e.EntityName)
                .ToListAsync();
        }

        public async Task<List<EntityMaster>> GetEntitiesByTypeAsync(string vendorType)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.EntityMasters
                .Where(e => e.VendorType == vendorType && e.EntityStatus == 'Y')
                .OrderBy(e => e.EntityName)
                .ToListAsync();
        }

        public async Task<EntityMaster> CreateEntityAsync(EntityMaster entity)
        {
            entity.CreatedDate = DateTime.Now;
            using var ctx = _dbFactory.CreateDbContext();
            ctx.EntityMasters.Add(entity);
            await ctx.SaveChangesAsync();
            return entity;
        }

        public async Task<EntityMaster> UpdateEntityAsync(EntityMaster entity)
        {
            entity.ModifiedDate = DateTime.Now;
            using var ctx = _dbFactory.CreateDbContext();
            ctx.EntityMasters.Update(entity);
            await ctx.SaveChangesAsync();
            return entity;
        }

        public async Task<AddressMaster> AddAddressAsync(AddressMaster address)
        {
            address.CreatedDate = DateTime.Now;
            using var ctx = _dbFactory.CreateDbContext();
            ctx.Addresses.Add(address);
            await ctx.SaveChangesAsync();
            return address;
        }

        public async Task<List<AddressMaster>> GetEntityAddressesAsync(long entityId)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Addresses
                .Where(a => a.EntityId == entityId && a.AddressStatus == 'Y')
                .OrderBy(a => a.AddressType == 'M' ? 0 : 1)
                .ToListAsync();
        }

        public async Task<List<EntityMaster>> GetAdjustersAsync()
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.EntityMasters
                .Where(e => (e.PartyType == "Adjuster" || e.VendorType == "Adjuster") && e.EntityStatus == 'Y')
                .OrderBy(e => e.EntityName)
                .ToListAsync();
        }
    }
}
