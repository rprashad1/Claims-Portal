using ClaimsPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    public interface IDatabaseEntityService
    {
        Task<EntityMaster?> GetEntityAsync(long entityId);
        Task<List<EntityMaster>> GetEntitiesByGroupAsync(string entityGroupCode);
        Task<List<EntityMaster>> GetEntitiesByTypeAsync(string vendorType);
        Task<EntityMaster> CreateEntityAsync(EntityMaster entity);
        Task<EntityMaster> UpdateEntityAsync(EntityMaster entity);
        Task<AddressMaster> AddAddressAsync(AddressMaster address);
        Task<List<AddressMaster>> GetEntityAddressesAsync(long entityId);
    }

    public class DatabaseEntityService : IDatabaseEntityService
    {
        private readonly ClaimsPortalDbContext _context;

        public DatabaseEntityService(ClaimsPortalDbContext context)
        {
            _context = context;
        }

        public async Task<EntityMaster?> GetEntityAsync(long entityId)
        {
            return await _context.EntityMasters
                .FirstOrDefaultAsync(e => e.EntityId == entityId && e.EntityStatus == 'Y');
        }

        public async Task<List<EntityMaster>> GetEntitiesByGroupAsync(string entityGroupCode)
        {
            return await _context.EntityMasters
                .Where(e => e.EntityGroupCode == entityGroupCode && e.EntityStatus == 'Y')
                .OrderBy(e => e.EntityName)
                .ToListAsync();
        }

        public async Task<List<EntityMaster>> GetEntitiesByTypeAsync(string vendorType)
        {
            return await _context.EntityMasters
                .Where(e => e.VendorType == vendorType && e.EntityStatus == 'Y')
                .OrderBy(e => e.EntityName)
                .ToListAsync();
        }

        public async Task<EntityMaster> CreateEntityAsync(EntityMaster entity)
        {
            entity.CreatedDate = DateTime.Now;
            _context.EntityMasters.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<EntityMaster> UpdateEntityAsync(EntityMaster entity)
        {
            entity.ModifiedDate = DateTime.Now;
            _context.EntityMasters.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<AddressMaster> AddAddressAsync(AddressMaster address)
        {
            address.CreatedDate = DateTime.Now;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<List<AddressMaster>> GetEntityAddressesAsync(long entityId)
        {
            return await _context.Addresses
                .Where(a => a.EntityId == entityId && a.AddressStatus == 'Y')
                .OrderBy(a => a.AddressType == 'M' ? 0 : 1)
                .ToListAsync();
        }
    }
}
