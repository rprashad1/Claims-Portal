using ClaimsPortal.Models;

namespace ClaimsPortal.Services;

public interface IClaimService
{
    Task<Claim> CreateClaimAsync(Claim claim);
    Task<Claim?> GetClaimAsync(string claimNumber);
    Task<List<Claim>> GetAllClaimsAsync();
    Task UpdateClaimAsync(Claim claim);
    string GenerateClaimNumber();
    string GenerateSubClaimId(string claimNumber, int subClaimIndex);
}

public interface IAdjusterService
{
    Task<List<Adjuster>> GetAllAdjustersAsync();
    Task<Adjuster?> GetAdjusterAsync(string adjusterId);
}

public interface ILookupService
{
    Task<List<ReportedBy>> GetReportedByListAsync();
    Task<List<ReportingMethod>> GetReportingMethodsAsync();
    Task<List<string>> GetCoverageTypesAsync();
    Task<List<string>> GetNatureOfInjuriesAsync();
    Task<List<string>> GetAuthorityTypesAsync();
}

public class MockClaimService : IClaimService
{
    private readonly List<Claim> _claims = [];
    private int _claimCounter = 1000;

    public Task<Claim> CreateClaimAsync(Claim claim)
    {
        claim.CreatedDate = DateTime.Now;
        PopulateSubClaims(claim);
        _claims.Add(claim);
        return Task.FromResult(claim);
    }

    public Task<Claim?> GetClaimAsync(string claimNumber)
    {
        var claim = _claims.FirstOrDefault(c => c.ClaimNumber == claimNumber);
        
        // If no claim found, return null; otherwise populate SubClaims if empty
        if (claim == null)
        {
            return Task.FromResult<Claim?>(null);
        }

        // Populate SubClaims if empty (for demo purposes)
        if (claim.SubClaims.Count == 0)
        {
            PopulateSubClaims(claim);
        }

        return Task.FromResult<Claim?>(claim);
    }

    private void PopulateSubClaims(Claim claim)
    {
        // Create sample sub-claims based on claim data
        var subClaims = new List<SubClaim>();

        // Sub-claim 1: BI (Bodily Injury)
        subClaims.Add(new SubClaim
        {
            Id = 1,
            FeatureNumber = 3,
            Coverage = "BI",
            CoverageLimits = "100,000",
            ClaimantName = claim.InsuredParty.Name,
            ExpenseReserve = 0.00m,
            IndemnityReserve = 1500.00m,
            AssignedAdjusterId = "ADJ001",
            AssignedAdjusterName = "Mary Sperling",
            Status = "Open",
            CreatedDate = claim.CreatedDate,
            ClaimType = "Driver"
        });

        // Sub-claim 2: PIP (Personal Injury Protection)
        if (claim.Passengers.Count > 0)
        {
            subClaims.Add(new SubClaim
            {
                Id = 2,
                FeatureNumber = 2,
                Coverage = "PIP",
                CoverageLimits = "50,000",
                ClaimantName = claim.Passengers[0].Name,
                ExpenseReserve = 250.00m,
                IndemnityReserve = 1000.00m,
                AssignedAdjusterId = "ADJ002",
                AssignedAdjusterName = "Pamela Baldwin",
                Status = "Open",
                CreatedDate = claim.CreatedDate,
                ClaimType = "Passenger"
            });
        }

        // Sub-claim 3: PD (Property Damage)
        subClaims.Add(new SubClaim
        {
            Id = 3,
            FeatureNumber = 1,
            Coverage = "PD",
            CoverageLimits = "10,000",
            ClaimantName = "Unknown Owner",
            ExpenseReserve = 0.00m,
            IndemnityReserve = 0.00m,
            AssignedAdjusterId = "ADJ003",
            AssignedAdjusterName = "Christine Wood",
            Status = "Closed",
            CreatedDate = claim.CreatedDate,
            ClaimType = "ThirdParty"
        });

        // Sub-claim 4: UM (Uninsured Motorist)
        subClaims.Add(new SubClaim
        {
            Id = 4,
            FeatureNumber = 4,
            Coverage = "UM",
            CoverageLimits = "25,000",
            ClaimantName = claim.InsuredParty.Name,
            ExpenseReserve = 0.00m,
            IndemnityReserve = 3500.00m,
            AssignedAdjusterId = "ADJ004",
            AssignedAdjusterName = "Lens Jacques",
            Status = "Open",
            CreatedDate = claim.CreatedDate,
            ClaimType = "Driver"
        });

        claim.SubClaims = subClaims;
    }

    public Task<List<Claim>> GetAllClaimsAsync()
    {
        return Task.FromResult(_claims.ToList());
    }

    public Task UpdateClaimAsync(Claim claim)
    {
        var existingClaim = _claims.FirstOrDefault(c => c.ClaimNumber == claim.ClaimNumber);
        if (existingClaim != null)
        {
            var index = _claims.IndexOf(existingClaim);
            _claims[index] = claim;
        }
        return Task.CompletedTask;
    }

    public string GenerateClaimNumber()
    {
        return $"CLM{DateTime.Now:yyyyMMdd}{++_claimCounter}";
    }

    public string GenerateSubClaimId(string claimNumber, int subClaimIndex)
    {
        return $"{claimNumber}-{subClaimIndex:D2}";
    }
}

public class MockAdjusterService : IAdjusterService
{
    private readonly List<Adjuster> _adjusters = [];

    public MockAdjusterService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _adjusters.Add(new Adjuster
        {
            Id = "ADJ001",
            FirstName = "Michael",
            LastName = "Anderson",
            Email = "michael.anderson@company.com",
            PhoneNumber = "(555) 111-1111",
            Territory = "Northern Territory",
            CurrentWorkload = 8
        });

        _adjusters.Add(new Adjuster
        {
            Id = "ADJ002",
            FirstName = "Emily",
            LastName = "Brown",
            Email = "emily.brown@company.com",
            PhoneNumber = "(555) 222-2222",
            Territory = "Central Territory",
            CurrentWorkload = 12
        });

        _adjusters.Add(new Adjuster
        {
            Id = "ADJ003",
            FirstName = "David",
            LastName = "Chen",
            Email = "david.chen@company.com",
            PhoneNumber = "(555) 333-3333",
            Territory = "Southern Territory",
            CurrentWorkload = 5
        });

        _adjusters.Add(new Adjuster
        {
            Id = "ADJ004",
            FirstName = "Jessica",
            LastName = "Martinez",
            Email = "jessica.martinez@company.com",
            PhoneNumber = "(555) 444-4444",
            Territory = "Western Territory",
            CurrentWorkload = 10
        });
    }

    public Task<List<Adjuster>> GetAllAdjustersAsync()
    {
        return Task.FromResult(_adjusters.ToList());
    }

    public Task<Adjuster?> GetAdjusterAsync(string adjusterId)
    {
        var adjuster = _adjusters.FirstOrDefault(a => a.Id == adjusterId);
        return Task.FromResult(adjuster);
    }
}

public class MockLookupService : ILookupService
{
    public Task<List<ReportedBy>> GetReportedByListAsync()
    {
        return Task.FromResult(new List<ReportedBy>
        {
            new ReportedBy { Id = 1, Name = "Insured" },
            new ReportedBy { Id = 2, Name = "Brokers" },
            new ReportedBy { Id = 3, Name = "Driver" },
            new ReportedBy { Id = 4, Name = "Claimant" },
            new ReportedBy { Id = 5, Name = "Other" }
        });
    }

    public Task<List<ReportingMethod>> GetReportingMethodsAsync()
    {
        return Task.FromResult(new List<ReportingMethod>
        {
            new ReportingMethod { Id = 1, Method = "Phone" },
            new ReportingMethod { Id = 2, Method = "Email" },
            new ReportingMethod { Id = 3, Method = "Mail" },
            new ReportingMethod { Id = 4, Method = "Fax" },
            new ReportingMethod { Id = 5, Method = "Agent Portal" },
            new ReportingMethod { Id = 6, Method = "Insured Portal" }
        });
    }

    public Task<List<string>> GetCoverageTypesAsync()
    {
        return Task.FromResult(new List<string> { "BI", "PD", "PIP", "APIP", "UM" });
    }

    public Task<List<string>> GetNatureOfInjuriesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "Back Sprain",
            "Leg Injury",
            "Head Injury",
            "Neck Injury (Whiplash)",
            "Fracture",
            "Internal Injury",
            "Burns",
            "Other"
        });
    }

    public Task<List<string>> GetAuthorityTypesAsync()
    {
        return Task.FromResult(new List<string> { "Police Department", "Fire Department" });
    }
}
