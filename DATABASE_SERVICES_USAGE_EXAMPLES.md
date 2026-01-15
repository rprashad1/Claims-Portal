# ?? **DATABASE SERVICES - QUICK START GUIDE**

## **4 Services Created** ?

| Service | Purpose | Usage |
|---------|---------|-------|
| `DatabaseLookupService` | Load lookup codes (Claimant types, Vendors, etc.) | `@inject DatabaseLookupService LookupService` |
| `IDatabasePolicyService` | Load/Save policies | `@inject IDatabasePolicyService PolicyService` |
| `IDatabaseEntityService` | Load/Save vendors, adjusters, addresses | `@inject IDatabaseEntityService EntityService` |
| `IDatabaseClaimService` | Load/Save FNOL, claims, vehicles, claimants | `@inject IDatabaseClaimService ClaimService` |

---

## **How to Use in Blazor Components**

### **Example 1: Get Lookup Codes**

```razor
@page "/test"
@inject DatabaseLookupService LookupService

<h3>Claimant Types from Database</h3>
<ul>
    @foreach (var code in claimantTypes)
    {
        <li>@code.RecordDescription (@code.RecordCode)</li>
    }
</ul>

@code {
    private List<LookupCode> claimantTypes = [];

    protected override async Task OnInitializedAsync()
    {
        claimantTypes = await LookupService.GetClaimantTypesAsync();
    }
}
```

---

### **Example 2: Get Active Policies**

```razor
@page "/policies"
@inject IDatabasePolicyService PolicyService

<h3>Active Policies</h3>
<table>
    <thead>
        <tr>
            <th>Policy Number</th>
            <th>Type</th>
            <th>Effective Date</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var policy in policies)
        {
            <tr>
                <td>@policy.PolicyNumber</td>
                <td>@policy.PolicyType</td>
                <td>@policy.EffectiveDate.ToShortDateString()</td>
            </tr>
        }
    </tbody>
</table>

@code {
    private List<Data.Policy> policies = [];

    protected override async Task OnInitializedAsync()
    {
        policies = await PolicyService.GetActivePoliciesAsync();
    }
}
```

---

### **Example 3: Create FNOL with Database**

```razor
@page "/create-fnol"
@inject IDatabaseClaimService ClaimService
@inject IDatabasePolicyService PolicyService

<h3>Create FNOL</h3>

<div class="form-group">
    <label>Policy Number:</label>
    <input type="text" @bind="policyNumber" />
</div>

<div class="form-group">
    <label>Date of Loss:</label>
    <input type="date" @bind="dateOfLoss" />
</div>

<button @onclick="CreateFnol" class="btn btn-primary">Create FNOL</button>

@if (!string.IsNullOrEmpty(message))
{
    <div class="alert alert-success">@message</div>
}

@code {
    private string policyNumber;
    private DateTime dateOfLoss = DateTime.Today;
    private string message;

    private async Task CreateFnol()
    {
        // Verify policy exists
        var policy = await PolicyService.GetPolicyAsync(policyNumber);
        if (policy == null)
        {
            message = "Policy not found";
            return;
        }

        // Create FNOL
        var fnol = new Fnol
        {
            PolicyNumber = policyNumber,
            DateOfLoss = dateOfLoss,
            ReportDate = DateTime.Now,
            CreatedBy = "current_user@company.com"
        };

        var createdFnol = await ClaimService.CreateFnolAsync(fnol);
        message = $"FNOL created: {createdFnol.FnolNumber}";
    }
}
```

---

### **Example 4: Load Vendors by Type**

```razor
@page "/vendors"
@inject IDatabaseEntityService EntityService

<h3>Medical Vendors</h3>
<select>
    @foreach (var vendor in medicalVendors)
    {
        <option value="@vendor.EntityId">@vendor.EntityName</option>
    }
</select>

@code {
    private List<EntityMaster> medicalVendors = [];

    protected override async Task OnInitializedAsync()
    {
        medicalVendors = await EntityService.GetEntitiesByTypeAsync("MED");
    }
}
```

---

## **Services Are Registered** ?

No additional configuration needed! Services are automatically available in any Blazor component.

---

## **Key Points**

? **All 4 services are ready to use**
? **Dependency injection configured**
? **Database connection active**
? **Mixed mode works** - Use database services alongside mocks

---

## **Next Steps**

1. **Run app**: Press F5
2. **Test services**: Try injecting in a test component
3. **Integrate into components**: Replace mock calls with real database calls
4. **Verify data**: Check SQL Server to confirm data is saved

---

**Status**: ?? **READY TO USE**
