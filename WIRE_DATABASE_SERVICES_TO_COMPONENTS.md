# ?? **FIX: WIRE DATABASE SERVICES TO COMPONENTS**

## ?? **The Problem**

Your components are still using **Mock services** instead of **Database services**.

```csharp
// ? This is MOCK (no database save)
[Inject]
private IClaimService ClaimService { get; set; }

// ? This is DATABASE (saves to database)
[Inject]
private IDatabaseClaimService ClaimService { get; set; }
```

---

## ?? **The Solution: Map Components to Database Services**

### **Step 1: Update FnolNew.razor**

Replace:
```csharp
[Inject]
private IClaimService ClaimService { get; set; } = null!;

[Inject]
private IPolicyService PolicyService { get; set; } = null!;
```

With:
```csharp
[Inject]
private IDatabaseClaimService DatabaseClaimService { get; set; } = null!;

[Inject]
private IDatabasePolicyService DatabasePolicyService { get; set; } = null!;

[Inject]
private DatabaseLookupService LookupService { get; set; } = null!;
```

Then update the `SubmitClaim` method to save to database:

```csharp
private async Task SubmitClaim()
{
    if (step5Component != null && await step5Component.ValidateAsync())
    {
        try
        {
            // Create FNOL in database
            var fnol = new Fnol
            {
                PolicyNumber = CurrentClaim.PolicyInfo.PolicyNumber,
                DateOfLoss = CurrentClaim.LossDetails.DateOfLoss,
                ReportDate = DateTime.Now,
                CreatedBy = "current_user@company.com",
                LossDescription = CurrentClaim.LossDetails.LossDescription
            };

            var createdFnol = await DatabaseClaimService.CreateFnolAsync(fnol);

            // Add vehicles
            foreach (var vehicle in CurrentClaim.ThirdParties)
            {
                var dbVehicle = new Vehicle
                {
                    FnolId = createdFnol.FnolId,
                    VehicleParty = "3rd Party",
                    VIN = vehicle.VIN,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    PlateNumber = vehicle.PlateNumber,
                    DamageDetails = vehicle.DamageDetails,
                    CreatedBy = "current_user@company.com"
                };
                await DatabaseClaimService.AddVehicleAsync(dbVehicle);
            }

            // Add claimants
            foreach (var claimant in CurrentClaim.Passengers)
            {
                var dbClaimant = new Claimant
                {
                    FnolId = createdFnol.FnolId,
                    ClaimantName = claimant.Name,
                    ClaimantType = claimant.PartyType,
                    InjuryDescription = claimant.InjuryDescription,
                    HasInjury = claimant.HasInjury,
                    CreatedBy = "current_user@company.com"
                };
                await DatabaseClaimService.AddClaimantAsync(dbClaimant);
            }

            CurrentClaim.ClaimNumber = createdFnol.FnolNumber;

            // Show success modal
            if (successModal != null)
                await successModal.ShowAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving FNOL: {ex.Message}");
        }
    }
}
```

---

### **Step 2: Update VendorMaster.razor**

Replace mock service with database service:

```csharp
[Inject]
private IDatabaseEntityService EntityService { get; set; } = null!;
```

Update vendor creation to save to database:

```csharp
private async Task SaveVendor()
{
    var entity = new EntityMaster
    {
        EntityName = VendorName,
        EntityType = 'B', // Business
        EntityGroupCode = "Vendor",
        VendorType = VendorType,
        Email = Email,
        HomeBusinessPhone = Phone,
        EntityStatus = 'Y',
        CreatedBy = "current_user@company.com"
    };

    var savedEntity = await EntityService.CreateEntityAsync(entity);

    if (!string.IsNullOrEmpty(Street))
    {
        var address = new AddressMaster
        {
            EntityId = savedEntity.EntityId,
            AddressType = 'M', // Main
            StreetAddress = Street,
            City = City,
            State = State,
            ZipCode = ZipCode,
            CreatedBy = "current_user@company.com"
        };

        await EntityService.AddAddressAsync(address);
    }

    message = $"Vendor '{VendorName}' saved successfully!";
    ClearForm();
}
```

---

### **Step 3: Update ClaimDetail.razor**

Load data from database instead of mock:

```csharp
@inject IDatabaseClaimService ClaimService
@inject DatabaseLookupService LookupService

protected override async Task OnInitializedAsync()
{
    if (!string.IsNullOrEmpty(ClaimNumber))
    {
        var fnol = await ClaimService.GetFnolByNumberAsync(ClaimNumber);
        if (fnol != null)
        {
            // Load claim data from database
            LoadClaimFromDatabase(fnol);
        }
    }
}

private void LoadClaimFromDatabase(Fnol fnol)
{
    // Populate UI from database records
    CurrentClaim.ClaimNumber = fnol.FnolNumber;
    CurrentClaim.LossDetails.DateOfLoss = fnol.DateOfLoss;
    CurrentClaim.LossDetails.LossDescription = fnol.LossDescription;
    // ... load other fields
}
```

---

## ?? **Complete Mapping Guide**

| Component | Current Service | Database Service | What to Inject |
|-----------|-----------------|------------------|----------------|
| **FnolNew.razor** | `IClaimService` | `IDatabaseClaimService` | `@inject IDatabaseClaimService` |
| **VendorMaster.razor** | `IVendorService` | `IDatabaseEntityService` | `@inject IDatabaseEntityService` |
| **ClaimDetail.razor** | `IClaimService` | `IDatabaseClaimService` | `@inject IDatabaseClaimService` |
| **Dashboard.razor** | `IClaimService` | `IDatabaseClaimService` | `@inject IDatabaseClaimService` |
| Any lookup dropdowns | `ILookupService` | `DatabaseLookupService` | `@inject DatabaseLookupService` |

---

## ?? **Quick Fix (Start Here)**

### **For FNOL Component:**

1. Open `Components/Pages/Fnol/FnolNew.razor`
2. Find the `@code` section
3. Replace the service injections
4. Update `SubmitClaim()` method to use database service

### **For Vendor Component:**

1. Open `Components/Pages/VendorMaster.razor`
2. Replace `IVendorService` with `IDatabaseEntityService`
3. Update save method

---

## ? **After Making Changes**

1. **Build** (Ctrl+Shift+B)
2. **Run** (F5)
3. **Create a claim** - Should now save to database
4. **Check SQL Server** - Query the FNOL table to verify data

```sql
SELECT * FROM ClaimsPortal.dbo.FNOL;
SELECT * FROM ClaimsPortal.dbo.EntityMaster;
```

---

**The issue is NOT the services - they exist and work!**
**The issue is the components aren't USING them yet.**

Apply these changes and data will start saving to the database! ??
