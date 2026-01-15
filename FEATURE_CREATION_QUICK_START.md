# ?? FEATURE CREATION MODAL - QUICK START GUIDE

## Implementation Checklist

Use this checklist to implement FeatureCreationModal in any component (Driver, Passenger, Third Party, Property Damage, etc.)

---

## ? Step 1: Add Modal Reference
```razor
<FeatureCreationModal @ref="featureCreationModal" 
                     OnFeatureCreated="OnFeatureCreated" 
                     OnCancelled="OnFeatureCreationCancelled"
                     OnCreationComplete="OnFeatureCreationComplete" />
```

---

## ? Step 2: Declare Modal Variable
```csharp
private FeatureCreationModal? featureCreationModal;
```

---

## ? Step 3: Show Modal When Ready
```csharp
// Example: After injury is captured for a driver
private async Task SaveDriverAndCreateFeature()
{
    if (IsDriverInjured)
    {
        if (featureCreationModal != null)
            await featureCreationModal.ShowAsync(Driver.Name, "Driver");
    }
}
```

---

## ? Step 4: Handle Feature Creation
```csharp
private void OnFeatureCreated(SubClaim subClaim)
{
    FeatureCounter++;
    subClaim.FeatureNumber = FeatureCounter;
    SubClaimsList.Add(subClaim);  // Add to your list
}
```

---

## ? Step 5: Handle Creation Complete
```csharp
private async Task OnFeatureCreationComplete()
{
    DriverSaved = true;  // or equivalent for your context
    ResetForm();  // Clear party info but keep features
}
```

---

## ? Step 6: Handle Cancellation
```csharp
private async Task OnFeatureCreationCancelled()
{
    // Mark as saved even if user cancels modal
    DriverSaved = true;
    ResetForm();
}
```

---

## ?? Complete Template Example

### For Any Party Type (Driver/Passenger/ThirdParty/etc.)

```razor
@page "/fnol/party-features"
@using ClaimsPortal.Models
@using ClaimsPortal.Components.Modals

<div class="container">
    <!-- Party Information Section -->
    <div class="card mb-4">
        <div class="card-header">
            <h6 class="mb-0">Party Information</h6>
        </div>
        <div class="card-body">
            <input type="text" class="form-control" @bind="PartyName" placeholder="Party Name" />
            <textarea class="form-control mt-3" rows="3" @bind="PartyDetails" placeholder="Details"></textarea>
            
            <button type="button" class="btn btn-primary mt-3" 
                    @onclick="ShowFeatureCreation"
                    disabled="@string.IsNullOrEmpty(PartyName)">
                <i class="bi bi-plus-circle"></i> Create Feature
            </button>
        </div>
    </div>

    <!-- Features Grid -->
    @if (Features.Count > 0)
    {
        <div class="card">
            <div class="card-header">
                <h6 class="mb-0">Created Features</h6>
            </div>
            <div class="card-body">
                <table class="table table-hover">
                    <thead>
                        <tr>
                            <th>Feature #</th>
                            <th>Coverage</th>
                            <th>Expense Reserve</th>
                            <th>Indemnity Reserve</th>
                            <th>Adjuster</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var feature in Features)
                        {
                            <tr>
                                <td><strong>@feature.FeatureNumber</strong></td>
                                <td>@feature.Coverage - @feature.CoverageLimits</td>
                                <td>$@feature.ExpenseReserve.ToString("F2")</td>
                                <td>$@feature.IndemnityReserve.ToString("F2")</td>
                                <td>@feature.AssignedAdjusterName</td>
                                <td>
                                    <button class="btn btn-sm btn-outline-danger" 
                                            @onclick="@(() => RemoveFeature(feature.Id))">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    }
</div>

<!-- Feature Creation Modal -->
<FeatureCreationModal @ref="featureCreationModal" 
                     OnFeatureCreated="OnFeatureCreated" 
                     OnCancelled="OnFeatureCreationCancelled"
                     OnCreationComplete="OnFeatureCreationComplete" />

@code {
    private FeatureCreationModal? featureCreationModal;
    
    private string PartyName = string.Empty;
    private string PartyDetails = string.Empty;
    private List<SubClaim> Features = new();
    private int FeatureCounter = 0;

    private async Task ShowFeatureCreation()
    {
        if (!string.IsNullOrEmpty(PartyName) && featureCreationModal != null)
        {
            await featureCreationModal.ShowAsync(PartyName, "Party");
        }
    }

    private void OnFeatureCreated(SubClaim subClaim)
    {
        FeatureCounter++;
        subClaim.FeatureNumber = FeatureCounter;
        Features.Add(subClaim);
    }

    private async Task OnFeatureCreationCancelled()
    {
        // Handle cancellation if needed
    }

    private async Task OnFeatureCreationComplete()
    {
        // Mark party as complete
        StateHasChanged();
    }

    private void RemoveFeature(int featureId)
    {
        Features.RemoveAll(f => f.Id == featureId);
        RenumberFeatures();
    }

    private void RenumberFeatures()
    {
        for (int i = 0; i < Features.Count; i++)
        {
            Features[i].FeatureNumber = i + 1;
        }
        FeatureCounter = Features.Count;
    }
}
```

---

## ?? Key Points

### Modal Parameters
```csharp
await featureCreationModal.ShowAsync(
    claimantName: "John Doe",      // Person/Property name
    claimType: "Driver"             // Type of claimant (optional)
);
```

### Feature Counter Pattern
```csharp
private int FeatureCounter = 0;

private void OnFeatureCreated(SubClaim subClaim)
{
    FeatureCounter++;
    subClaim.FeatureNumber = FeatureCounter;
    Features.Add(subClaim);
}
```

### Auto-Renumbering on Delete
```csharp
private void RenumberFeatures()
{
    for (int i = 0; i < Features.Count; i++)
    {
        Features[i].FeatureNumber = i + 1;
    }
    FeatureCounter = Features.Count;
}
```

---

## ?? Usage Examples

### Driver Injury Feature
```csharp
await featureCreationModal.ShowAsync("Insured", "Driver");
```

### Passenger Injury Feature
```csharp
await featureCreationModal.ShowAsync("Jane Doe", "Passenger");
```

### Third Party Injury Feature
```csharp
await featureCreationModal.ShowAsync("John Smith", "ThirdParty");
```

### Property Damage Feature
```csharp
await featureCreationModal.ShowAsync("123 Main St, Apt 4B", "PropertyDamage");
```

---

## ? Verification Checklist

- [ ] Modal reference declared
- [ ] Modal component added to page
- [ ] ShowAsync called with claimant name and type
- [ ] OnFeatureCreated callback implemented
- [ ] OnFeatureCreationComplete callback implemented
- [ ] OnFeatureCreationCancelled callback implemented
- [ ] Feature counter incremented
- [ ] Features added to list
- [ ] Grid displays created features
- [ ] Remove feature functionality works
- [ ] Feature renumbering works correctly

---

## ?? Troubleshooting

**Problem**: Modal doesn't show
- **Solution**: Ensure `featureCreationModal != null` before calling `ShowAsync()`

**Problem**: Features not appearing in grid
- **Solution**: Verify `OnFeatureCreated` callback adds feature to list and increments counter

**Problem**: Feature numbers are wrong after delete
- **Solution**: Call `RenumberFeatures()` after removing feature

**Problem**: Form not resetting for second feature
- **Solution**: Modal auto-resets when user clicks "Yes, Add Another"

---

## ?? Component Export

The `FeatureCreationModal.razor` is production-ready and can be:
- ? Used in FnolStep3 (Driver)
- ? Used in FnolStep3 (Passengers)
- ? Used in FnolStep4 (Third Parties)
- ? Used in FnolStep4 (Property Damage)
- ? Used in Claim Detail (Edit Features)
- ? Reused anywhere features need to be created

---

**Status**: ? READY FOR PRODUCTION
**Last Updated**: Today
