# Claims Portal - Developer Quick Reference Guide

## Quick Start

### Running the Application
```bash
cd C:\Projects\Claims\ClaimsPortal
dotnet run
```
Navigate to: `https://localhost:5001` or `http://localhost:5000`

### Database Connection
```json
// appsettings.json
"ConnectionStrings": {
    "ClaimsPortal": "Server=localhost;Database=ClaimsPortal;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## Key Navigation Routes

| Route | Component | Purpose |
|-------|-----------|---------|
| `/` | Dashboard.razor | Main dashboard |
| `/fnol` | FnolSearch.razor | FNOL/Claim search |
| `/fnol/new` | FnolNew.razor | Create new FNOL |
| `/fnol/edit/{FnolNumber}` | FnolNew.razor | Edit existing FNOL |
| `/claim/{ClaimNumber}` | ClaimDetail.razor | View claim details |
| `/vendors` | VendorMaster.razor | Vendor management |

---

## Service Injection Patterns

### For Database Operations (Recommended)
```csharp
@inject IDatabaseClaimService DatabaseClaimService
@inject IDatabasePolicyService DatabasePolicyService
@inject DatabaseLookupService LookupService
```

### For Mock Data (Development Only)
```csharp
@inject IClaimService ClaimService
@inject IPolicyService PolicyService
```

---

## Common Code Patterns

### Loading Claim Data
```csharp
private async Task LoadClaimAsync()
{
    if (!string.IsNullOrEmpty(ClaimNumber))
    {
        Claim = await DatabaseClaimService.GetClaimWithDetailsAsync(ClaimNumber);
    }
}
```

### Searching Claims
```csharp
var results = await DatabaseClaimService.SearchClaimsAsync(
    claimNumber: searchClaimNumber,
    fnolNumber: null,
    policyNumber: searchPolicyNumber,
    dateOfLossFrom: dateFrom,
    dateOfLossTo: dateTo,
    status: selectedStatus
);
```

### Saving FNOL Draft
```csharp
var fnol = new Fnol
{
    FnolNumber = CurrentFnolNumber,
    PolicyNumber = CurrentClaim.PolicyInfo?.PolicyNumber ?? "",
    DateOfLoss = CurrentClaim.LossDetails.DateOfLoss,
    // ... other fields
};
await DatabaseClaimService.SaveFnolAsDraftAsync(fnol);
```

### Creating Entity (Witness, Authority, etc.)
```csharp
var entity = await DatabaseClaimService.CreateEntityAsync(new EntityMaster
{
    EntityType = 'I',           // Individual
    EntityGroupCode = "Witness",
    PartyType = "Witness",
    EntityName = witnessName,
    HomeBusinessPhone = phone,
    Email = email,
    EntityStatus = 'Y',
    CreatedBy = "system_user"
});
```

---

## Reusable Components

### Address Template
```razor
<AddressTemplate @bind-Address="partyAddress" 
                 AllFieldsOptional="true" 
                 ShowCounty="true" />
```

### Injury Template
```razor
<InjuryTemplateUnified @bind-Injury="driverInjury" />
```

### Section Card
```razor
<SectionCard Title="Loss Details" 
             SubTitle="Enter loss information"
             StepNumber="1"
             OnNext="HandleNext"
             OnPrevious="HandlePrevious"
             IsNextDisabled="@(!IsValid)">
    <!-- Content here -->
</SectionCard>
```

---

## Modal Usage Patterns

### Opening a Modal
```csharp
private WitnessModal? witnessModal;

private async Task OpenWitnessModal()
{
    if (witnessModal != null)
        await witnessModal.ShowAsync();
}
```

### Handling Modal Result
```razor
<WitnessModal @ref="witnessModal" 
              OnSave="HandleWitnessSaved" />

@code {
    private void HandleWitnessSaved(Witness witness)
    {
        Witnesses.Add(witness);
        StateHasChanged();
    }
}
```

---

## Status Badge Classes

```csharp
private string GetStatusBadgeClass(string status) => status switch
{
    "Active" or "Open" => "bg-success",
    "Closed" => "bg-secondary",
    "Draft" => "bg-warning text-dark",
    "Inactive" or "Cancelled" => "bg-danger",
    _ => "bg-info"
};
```

---

## Database Entity Mappings

### FNOL Status Codes
| Code | Meaning |
|------|---------|
| O | Open |
| C | Closed/Converted |
| D | Draft |

### Policy Status Codes
| Code | Meaning |
|------|---------|
| Y | Active |
| N | Inactive |
| C | Cancelled |
| E | Expired |

### Entity Types
| Code | Meaning |
|------|---------|
| I | Individual |
| B | Business |

### Vehicle Party Types
| Code | Meaning |
|------|---------|
| IPV | Insured Policy Vehicle |
| TPV | Third Party Vehicle |

---

## FNOL Number Format
```
FNOL{YYYYMMDD}{SEQUENCE}
Example: FNOL2025010800001
```

## Claim Number Format
```
CLM{YYYYMMDD}{SEQUENCE}
Example: CLM2025010800001
```

---

## Testing Tips

### Quick FNOL Creation
1. Navigate to `/fnol`
2. Click "New FNOL"
3. Fill required fields (marked with *)
4. Click "Save Progress" to save draft
5. Complete all steps and click "Submit Claim"

### Quick Claim Search
1. Navigate to `/fnol`
2. Enter search criteria
3. Click "Search"
4. Click "View" on any result

---

## Troubleshooting

### "Claim not found" Error
- Verify claim/FNOL number exists in database
- Check if using correct service (DatabaseClaimService vs MockClaimService)
- Verify database connection string

### Policy Verification Not Working
- Check if policy data exists in database
- Verify PolicyNumber format matches database
- Check DatabasePolicyService is properly injected

### Draft Not Saving
- Verify all required fields have values
- Check for database constraint errors in console
- Ensure FnolNumber is generated

---

## Key Files Quick Reference

| Purpose | File |
|---------|------|
| Main Layout | `Components/Layout/MainLayout.razor` |
| Navigation | `Components/Layout/NavMenu.razor` |
| FNOL Wizard | `Components/Pages/Fnol/FnolNew.razor` |
| Claim View | `Components/Pages/Claim/ClaimDetail.razor` |
| Database Context | `Data/ClaimsPortalDbContext.cs` |
| Claim Service | `Services/DatabaseClaimService.cs` |
| Claim Model | `Models/Claim.cs` |
| Injury Model | `Models/Injury.cs` |
| App Entry | `Program.cs` |
| Styles | `wwwroot/app.css` |

---

## Build Commands

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run with hot reload
dotnet watch run

# Publish for production
dotnet publish -c Release -o ./publish
```

---

**Last Updated:** January 2025
