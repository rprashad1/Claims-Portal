using System;

using Data = ClaimsPortal.Data;
using Models = ClaimsPortal.Models;

namespace ClaimsPortal.Services
{
    /// <summary>
    /// Small helper to encapsulate mapping between UI `Policy` and data `Fnol` policy fields.
    /// Keeps mapping logic in one place for easier testing and refactor.
    /// </summary>
    public static class PolicyMapper
    {
        public static void ApplyPolicyToFnol(Models.Policy? policy, Data.Fnol fnol)
        {
            if (policy == null) return;

            fnol.PolicyNumber = policy.PolicyNumber ?? string.Empty;
            fnol.PolicyEffectiveDate = policy.EffectiveDate;
            fnol.PolicyExpirationDate = policy.ExpiryDate;
            fnol.PolicyCancelDate = policy.CancelDate;
            fnol.PolicyStatus = policy.Status == "Active" ? 'Y' :
                                policy.Status == "Cancelled" ? 'C' :
                                policy.Status == "Expired" ? 'E' : (char?)'N';
        }

        public static Models.Policy MapFnolToPolicy(Data.Fnol fnol)
        {
            return new Models.Policy
            {
                PolicyNumber = fnol.PolicyNumber ?? string.Empty,
                EffectiveDate = fnol.PolicyEffectiveDate ?? DateTime.MinValue,
                ExpiryDate = fnol.PolicyExpirationDate ?? DateTime.MinValue,
                CancelDate = fnol.PolicyCancelDate,
                Status = fnol.PolicyStatus switch
                {
                    'Y' => "Active",
                    'C' => "Cancelled",
                    'E' => "Expired",
                    _ => "Unknown"
                },
                InsuredName = fnol.InsuredName ?? string.Empty,
                PhoneNumber = fnol.InsuredPhone ?? string.Empty,
                Email = fnol.InsuredEmail ?? string.Empty,
                Address = fnol.InsuredAddress ?? string.Empty,
                Address2 = fnol.InsuredAddress2 ?? string.Empty,
                City = fnol.InsuredCity ?? string.Empty,
                State = fnol.InsuredState ?? string.Empty,
                ZipCode = fnol.InsuredZipCode ?? string.Empty,
                DoingBusinessAs = fnol.InsuredDoingBusinessAs ?? string.Empty
            };
        }
    }
}
