using ClaimsPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    public interface IDatabasePolicyService
    {
        Task<Data.Policy?> GetPolicyAsync(string policyNumber);
        Task<List<Data.Policy>> GetActivePoliciesAsync();
        Task<Data.Policy> CreatePolicyAsync(Data.Policy policy);
        Task<Data.Policy> UpdatePolicyAsync(Data.Policy policy);
    }

    public class DatabasePolicyService : IDatabasePolicyService
    {
        private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> _dbFactory;

        public DatabasePolicyService(Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Data.Policy?> GetPolicyAsync(string policyNumber)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Policies
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }

        public async Task<List<Data.Policy>> GetActivePoliciesAsync()
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Policies
                .Where(p => p.PolicyStatus == 'Y')
                .OrderBy(p => p.PolicyNumber)
                .ToListAsync();
        }

        public async Task<Data.Policy> CreatePolicyAsync(Data.Policy policy)
        {
            policy.CreatedDate = DateTime.Now;
            using var ctx = _dbFactory.CreateDbContext();
            ctx.Policies.Add(policy);
            await ctx.SaveChangesAsync();
            return policy;
        }

        public async Task<Data.Policy> UpdatePolicyAsync(Data.Policy policy)
        {
            policy.ModifiedDate = DateTime.Now;
            using var ctx = _dbFactory.CreateDbContext();
            ctx.Policies.Update(policy);
            await ctx.SaveChangesAsync();
            return policy;
        }
    }
}
