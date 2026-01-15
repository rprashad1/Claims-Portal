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
        private readonly ClaimsPortalDbContext _context;

        public DatabasePolicyService(ClaimsPortalDbContext context)
        {
            _context = context;
        }

        public async Task<Data.Policy?> GetPolicyAsync(string policyNumber)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }

        public async Task<List<Data.Policy>> GetActivePoliciesAsync()
        {
            return await _context.Policies
                .Where(p => p.PolicyStatus == 'Y')
                .OrderBy(p => p.PolicyNumber)
                .ToListAsync();
        }

        public async Task<Data.Policy> CreatePolicyAsync(Data.Policy policy)
        {
            policy.CreatedDate = DateTime.Now;
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<Data.Policy> UpdatePolicyAsync(Data.Policy policy)
        {
            policy.ModifiedDate = DateTime.Now;
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
            return policy;
        }
    }
}
