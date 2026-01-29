using ClaimsPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    /// <summary>
    /// Database-based lookup service for loading lookup codes from ClaimsPortal database
    /// </summary>
    public class DatabaseLookupService
    {
        private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> _dbFactory;

        public DatabaseLookupService(Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<LookupCode>> GetLookupCodesAsync(string recordType)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.LookupCodes
                .Where(l => l.RecordType == recordType && l.RecordStatus == 'Y')
                .OrderBy(l => l.SortOrder ?? 0)
                .ToListAsync();
        }

        public async Task<LookupCode?> GetLookupCodeAsync(string recordType, string recordCode)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.LookupCodes
                .FirstOrDefaultAsync(l =>
                    l.RecordType == recordType &&
                    l.RecordCode == recordCode &&
                    l.RecordStatus == 'Y');
        }

        public async Task<List<LookupCode>> GetClaimantTypesAsync()
        {
            return await GetLookupCodesAsync("Claimant");
        }

        public async Task<List<LookupCode>> GetVendorTypesAsync()
        {
            return await GetLookupCodesAsync("VendorType");
        }

        public async Task<List<LookupCode>> GetTransactionTypesAsync()
        {
            return await GetLookupCodesAsync("TransactionType");
        }

        /// <summary>
        /// Get Close Reason lookup codes for closing sub-claims/features
        /// </summary>
        public async Task<List<LookupCode>> GetCloseReasonsAsync()
        {
            return await GetLookupCodesAsync("CloseReason");
        }

        /// <summary>
        /// Get Reopen Reason lookup codes for reopening sub-claims/features
        /// </summary>
        public async Task<List<LookupCode>> GetReopenReasonsAsync()
        {
            return await GetLookupCodesAsync("ReopenReason");
        }
    }
}
