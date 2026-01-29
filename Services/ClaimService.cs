using ClaimsPortal.Models;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    private readonly LetterService? _letterService;
    private readonly LetterConfigStore? _configStore;

    public MockClaimService(LetterService? letterService = null, LetterConfigStore? configStore = null)
    {
        _letterService = letterService;
        _configStore = configStore;
    }

    public async Task<Claim> CreateClaimAsync(Claim claim)
    {
        claim.CreatedDate = DateTime.Now;
        PopulateSubClaims(claim);
        _claims.Add(claim);

            try
            {
                if (_letterService != null && _configStore != null)
                {
                    await GenerateLettersForClaimAsync(claim, null);
                }
            }
        catch
        {
            // Don't block claim creation if letter generation fails
        }

        return claim;
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
        if ((claim.SubClaims?.Count ?? 0) == 0)
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
                ClaimantName = claim.InsuredParty?.Name ?? string.Empty,
            ExpenseReserve = 0.00m,
            IndemnityReserve = 1500.00m,
            AssignedAdjusterId = "ADJ001",
            AssignedAdjusterName = "Mary Sperling",
            Status = "Open",
            CreatedDate = claim.CreatedDate,
            ClaimType = "Driver"
        });

        // Sub-claim 2: PIP (Personal Injury Protection)
        if ((claim.Passengers?.Count ?? 0) > 0)
        {
            subClaims.Add(new SubClaim
            {
                Id = 2,
                FeatureNumber = 2,
                Coverage = "PIP",
                CoverageLimits = "50,000",
                    ClaimantName = claim.Passengers?[0]?.Name ?? string.Empty,
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
                ClaimantName = claim.InsuredParty?.Name ?? string.Empty,
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

    private async Task GenerateLettersForClaimAsync(Claim claim, IEnumerable<string>? selectedRuleIds = null)
    {
    var rules = _configStore?.GetAll() ?? Array.Empty<LetterRule>();
        var generatedDir = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedLetters");
        Directory.CreateDirectory(generatedDir);

        HashSet<string>? selected = null;
        if (selectedRuleIds != null)
        {
            selected = new HashSet<string>(selectedRuleIds, StringComparer.OrdinalIgnoreCase);
        }

        var hasAttorney = (claim.DriverAttorney != null)
                 || (claim.Passengers?.Any(p => p.HasAttorney) ?? false)
                 || (claim.ThirdParties?.Any(t => t.HasAttorney) ?? false);

        foreach (var sub in (claim.SubClaims ?? Enumerable.Empty<SubClaim>()))
        {
                foreach (var rule in rules)
            {
                    var ruleId = rule?.Id ?? string.Empty;
                    var ruleTemplate = rule?.TemplateFile ?? string.Empty;
                    var docName = rule?.DocumentName ?? string.Empty;
                var ruleCoverage = rule?.Coverage ?? string.Empty;
                var subCoverage = sub?.Coverage ?? string.Empty;
                if (!string.Equals(ruleCoverage, subCoverage, StringComparison.OrdinalIgnoreCase))
                    continue;

                var ruleHasAttorney = rule?.HasAttorney ?? false;
                if (ruleHasAttorney != hasAttorney)
                    continue;

                bool claimantMatches = false;
                static string NormalizeForMatch(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : Regex.Replace(s, "[^A-Za-z0-9]", "").ToLowerInvariant();

                var ruleClaimNorm = NormalizeForMatch(rule?.Claimant);
                var subTypeNorm = NormalizeForMatch(sub?.ClaimType);
                var subNameNorm = NormalizeForMatch(sub?.ClaimantName);

                if (!string.IsNullOrEmpty(ruleClaimNorm))
                {
                    if (ruleClaimNorm.Contains("driver") && subTypeNorm.Contains("driver"))
                        claimantMatches = true;
                    else if (ruleClaimNorm.Contains("passenger") && subTypeNorm.Contains("passenger"))
                        claimantMatches = true;
                    else if (ruleClaimNorm.Contains("third") && subTypeNorm.Contains("third"))
                        claimantMatches = true;
                    else if (ruleClaimNorm == subNameNorm)
                        claimantMatches = true;
                    else if (ruleClaimNorm == NormalizeForMatch(sub?.ClaimType))
                        claimantMatches = true;
                }

                if (!claimantMatches)
                    continue;

                // Respect user's selection if provided
                if (selected != null && !selected.Contains(ruleId))
                    continue;

                var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "ClaimNumber", claim.ClaimNumber ?? string.Empty },
                    { "PolicyNumber", claim.PolicyInfo?.PolicyNumber ?? string.Empty },
                    { "InsuredName", claim.PolicyInfo?.InsuredName ?? claim.InsuredParty?.Name ?? string.Empty },
                    { "ClaimantName", sub?.ClaimantName ?? string.Empty },
                    { "LossDate", claim.LossDetails != null ? claim.LossDetails.DateOfLoss.ToString("yyyy-MM-dd") : string.Empty },
                    { "LocationAddress", claim.LossDetails?.Location ?? string.Empty },
                    { "AddresseeName", rule?.MailTo ?? claim.InsuredParty?.Name ?? sub?.ClaimantName ?? string.Empty },
                    { "AddresseeAddressLine1", claim.InsuredParty?.Address?.StreetAddress ?? string.Empty },
                    { "AddresseeAddressLine2", claim.InsuredParty?.Address?.AddressLine2 ?? string.Empty },
                    { "AddresseeCity", claim.InsuredParty?.Address?.City ?? string.Empty },
                    { "AddresseeState", claim.InsuredParty?.Address?.State ?? string.Empty },
                    { "AddresseePostalCode", claim.InsuredParty?.Address?.ZipCode ?? string.Empty },
                    { "AdjusterName", !string.IsNullOrEmpty(sub?.AssignedAdjusterName) ? sub.AssignedAdjusterName : "Claims Adjuster" },
                    { "AdjusterPhone", claim.PolicyInfo?.PhoneNumber ?? claim.InsuredParty?.PhoneNumber ?? string.Empty },
                    { "AdjusterEmail", claim.PolicyInfo?.ContactEmail ?? claim.InsuredParty?.Email ?? string.Empty },
                    { "TemplateName", rule?.TemplateFile ?? string.Empty },
                    { "RuleName", rule?.DocumentName ?? string.Empty },
                    { "GeneratedAt", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'") }
                };

                // Decide template: prefer explicit TemplateFile, else derive from DocumentName
                string? templateName = null;
                if (!string.IsNullOrWhiteSpace(ruleTemplate))
                {
                    templateName = ruleTemplate;
                }
                else if (!string.IsNullOrWhiteSpace(docName))
                {
                    var safe = string.Join("_", docName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Replace(' ', '_');
                    templateName = safe + ".html";
                }

                // Verify template exists under wwwroot/templates; skip if missing
                var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
                var templatePath = Path.Combine(templatesDir, templateName ?? string.Empty);
                if (string.IsNullOrWhiteSpace(templateName) || !File.Exists(templatePath))
                {
                    // Template missing for this rule, skip generation in mock path
                    continue;
                }

                var outDir = !string.IsNullOrWhiteSpace(rule?.Location) ? rule.Location : generatedDir;
                try
                {
                    Directory.CreateDirectory(outDir);
                }
                catch
                {
                    outDir = generatedDir;
                    Directory.CreateDirectory(outDir);
                }

                var safeDocName = string.Join("_", (docName ?? string.Empty).Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Replace(' ', '_');
                var filename = $"{safeDocName}_{claim.ClaimNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var outPath = Path.Combine(outDir, filename);

                if (_letterService == null)
                {
                    // LetterService not available in mock context; skip generation
                    continue;
                }

                await _letterService.GeneratePdfFromTemplateAsync(templateName, placeholders, outPath);

                var copyTo = Path.Combine(generatedDir, Path.GetFileName(outPath));
                try
                {
                    if (!string.Equals(Path.GetFullPath(outPath), Path.GetFullPath(copyTo), StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(outPath, copyTo, overwrite: true);
                    }
                }
                catch
                {
                    // ignore copy errors
                }
            }
        }
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
