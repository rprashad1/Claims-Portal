# Passenger & Third Party Feature/Sub-Claim Workflow

## ?? Overview

The Claims Portal now includes a complete sub-claim/feature creation workflow for all injury parties:
- ? **Driver Injuries** (Already implemented)
- ? **Passenger Injuries** (NEW - Step 3)
- ? **Third Party Injuries** (NEW - Step 4)

---

## ?? Feature Overview

### What Was Missing
The passenger and third-party workflows had no feature/sub-claim creation capability. Users could enter injury information but couldn't automatically create features for coverage and reserve allocation.

### What Changed
Now when any injured party is added (passenger or third party), the system automatically opens a feature creation modal to:
1. Select coverage type
2. Enter expense reserve
3. Enter indemnity reserve
4. Assign adjuster

---

## ?? Passenger Workflow (Step 3)

### Flow Diagram
```
User clicks "Add Passenger"
        ?
PassengerModal Opens
        ?
Enter Passenger Name
        ?
Select "Yes/No" for Injured
        ?
If Injured:
  ?? Nature of Injury (required)
  ?? Date of Medical Treatment (required)
  ?? Injury Description (required)
  ?? Fatality checkbox
        ?
Select "Yes/No" for Attorney
        ?
If Attorney:
  ?? Attorney Name (required)
  ?? Firm Name (required)
        ?
Click "Save & Create Feature"
        ?
If Injured:
  SubClaimModal Opens Automatically
        ?
  Select Coverage Type *
  Enter Expense Reserve *
  Enter Indemnity Reserve *
  Select Adjuster *
        ?
  Click "Create Feature"
        ?
  Feature added to grid
        ?
Passenger appears in Passenger List
Feature appears in Feature Grid
```

### Implementation Details

#### PassengerModal.razor (Updated)
```razor
<!-- New Alert -->
@if (Passenger.WasInjured)
{
    <div class="alert alert-warning mb-3">
        <i class="bi bi-exclamation-circle"></i> 
        A feature/sub-claim will be created for this injury after you save.
    </div>
}

<!-- Enhanced Validation -->
private bool IsFormValid()
{
    // Name always required
    // If injured: Nature, Date, Description required
    // If attorney: Name, Firm required
}

<!-- Button Updated -->
<button @onclick="OnAdd" disabled="@(!IsFormValid())">
    <i class="bi bi-check-circle"></i> Save & Create Feature
</button>
```

#### FnolStep3_DriverAndInjury.razor (Updated)
```csharp
// New Modal Reference
private SubClaimModal? subClaimModal;
private string CurrentPassengerName = string.Empty;

// New Method
private async Task AddPassengerAndCreateFeature(InsuredPassenger passenger)
{
    Passengers.Add(passenger);
    
    // If passenger is injured, create feature automatically
    if (passenger.WasInjured && passenger.InjuryInfo != null)
    {
        CurrentPassengerName = passenger.Name;
        if (subClaimModal != null)
            await subClaimModal.ShowAsync();
    }
}

// Updated RemovePassenger
private void RemovePassenger(string name)
{
    Passengers.RemoveAll(p => p.Name == name);
    
    // Also remove associated features
    var passengerFeatures = DriverSubClaims
        .Where(f => f.ClaimantName == name && f.ClaimType == "Passenger")
        .ToList();
    foreach (var feature in passengerFeatures)
    {
        DriverSubClaims.Remove(feature);
    }
    RenumberFeatures();
}
```

---

## ?? Third Party Workflow (Step 4)

### Flow Diagram
```
User clicks "Add Third Party"
        ?
ThirdPartyModal Opens
        ?
Select Party Type * (Vehicle, Pedestrian, Bicyclist, Property, Other)
        ?
Enter Name *
        ?
If Vehicle:
  ?? VIN
  ?? Year
  ?? Make
  ?? Model
  ?? Driver Info
        ?
Select "Yes/No" for Injured *
        ?
If Injured:
  ?? Nature of Injury (required)
  ?? Injury Description (required)
  ?? Date of Medical Treatment
  ?? Fatality checkbox
        ?
Select "Yes/No" for Attorney *
        ?
If Attorney:
  ?? Attorney Name (required)
  ?? Firm Name (required)
        ?
Click "Save & Create Feature"
        ?
If Injured:
  SubClaimModal Opens Automatically
        ?
  Select Coverage Type *
  Enter Expense Reserve *
  Enter Indemnity Reserve *
  Select Adjuster *
        ?
  Click "Create Feature"
        ?
  Feature added to grid
        ?
Third Party appears in Third Party List
Feature appears in Feature Grid
```

### Implementation Details

#### ThirdPartyModal.razor (Updated)
```razor
<!-- New Party Type Options -->
<option value="Vehicle">Third Party Vehicle</option>
<option value="Pedestrian">Pedestrian</option>
<option value="Bicyclist">Bicyclist</option>  <!-- NEW -->
<option value="Property">Property/Building</option>
<option value="Other">Other</option>

<!-- New Alert for Injuries -->
@if (ThirdParty.WasInjured)
{
    <div class="alert alert-warning mb-3">
        <i class="bi bi-exclamation-circle"></i> 
        A feature/sub-claim will be created for this injury after you save.
    </div>
}

<!-- Enhanced Validation -->
private bool IsFormValid()
{
    // Name and Type always required
    // If injured: Nature, Description required
    // If attorney: Name, Firm required
}

<!-- Button Updated -->
<button @onclick="OnAdd" disabled="@(!IsFormValid())">
    <i class="bi bi-check-circle"></i> Save & Create Feature
</button>
```

#### FnolStep4_ThirdParties.razor (Updated - NEW)
```csharp
// New Features
private List<SubClaim> ThirdPartySubClaims = [];
private string CurrentThirdPartyName = string.Empty;
private int FeatureCounter = 0;

// Feature Grid Display (NEW)
@if (ThirdPartySubClaims.Count > 0)
{
    <div class="card mb-4">
        <!-- Third Party Features Grid with Edit/Delete -->
    </div>
}

// New Method
private async Task AddThirdPartyAndCreateFeature(ThirdParty party)
{
    ThirdParties.Add(party);
    
    if (party.WasInjured && party.InjuryInfo != null)
    {
        CurrentThirdPartyName = party.Name;
        if (subClaimModal != null)
            await subClaimModal.ShowAsync();
    }
}

// Feature Management
public List<SubClaim> GetThirdPartySubClaims() => ThirdPartySubClaims;

private void AddOrUpdateSubClaim(SubClaim subClaim) { /* ... */ }
private void RemoveFeature(int id) { /* ... */ }
private void RenumberFeatures() { /* ... */ }
```

---

## ?? Feature Grid Display

Both passengers and third parties show feature grids when features are created:

```
?????????????????????????????????????????????????????????????????????????????????????????????????
? Feature ? Coverage/Limits  ? Claimant     ? Expense Res.   ? Indemnity Res.   ? Adjuster      ?
?????????????????????????????????????????????????????????????????????????????????????????????????
? 01      ? BI - 100/300     ? John Smith   ? $5,000.00      ? $25,000.00       ? Raj           ?
?         ?                  ?              ?                ?                  ? ?? ???         ?
?????????????????????????????????????????????????????????????????????????????????????????????????
? 02      ? PD - 50,000      ? Jane Doe     ? $10,000.00     ? $50,000.00       ? Edwin         ?
?         ?                  ?              ?                ?                  ? ?? ???         ?
?????????????????????????????????????????????????????????????????????????????????????????????????
```

### Features
- ? Auto-numbered features (01, 02, etc.)
- ? Currency formatting for reserves
- ? Edit functionality (updates values in place)
- ? Delete functionality (removes feature, renumbers others)
- ? Auto-renumbering when features deleted

---

## ?? Data Flow Integration

### Step 3 ? Step 4 (GoToStep4)
```csharp
// Collect driver sub-claims only
var driverSubClaims = step3Component
    .GetDriverSubClaims()
    .Where(f => f.ClaimType == "Driver")
    .ToList();
CurrentClaim.SubClaims.AddRange(driverSubClaims);
```

### Step 4 ? Step 5 (GoToStep5)
```csharp
// Collect passenger sub-claims from step 3
if (step3Component != null)
{
    var passengerSubClaims = step3Component.GetPassengerSubClaims();
    CurrentClaim.SubClaims.AddRange(passengerSubClaims);
}

// Collect third party sub-claims from step 4
var thirdPartySubClaims = step4Component.GetThirdPartySubClaims();
CurrentClaim.SubClaims.AddRange(thirdPartySubClaims);
```

### Final Claim Submission
All sub-claims are submitted together in the CurrentClaim.SubClaims list:
- Driver features (ClaimType = "Driver")
- Passenger features (ClaimType = "Passenger")
- Third Party features (ClaimType = "ThirdParty")

---

## ?? Testing Scenarios

### Passenger Scenario 1: Non-Injured Passenger
```
? Enter passenger name: "John Smith"
? Select "No" for injured
? Select "No" for attorney
? Click "Save & Create Feature"
? Verify: No feature modal opens
? Verify: Passenger appears in list
? Verify: No feature created
```

### Passenger Scenario 2: Injured with Attorney
```
? Enter passenger name: "Jane Doe"
? Select "Yes" for injured
? Fill: Nature (Whiplash), Date, Description
? Select "Yes" for attorney
? Fill: Attorney Name, Firm Name
? Click "Save & Create Feature"
? Verify: Feature modal opens
? Select coverage "BI"
? Enter reserves: 5000, 25000
? Select adjuster "Raj"
? Click "Create Feature"
? Verify: Passenger in list, Feature 01 in grid
```

### Third Party Scenario 1: Pedestrian (Injured)
```
? Select type: "Pedestrian"
? Enter name: "Mary Brown"
? Select "Yes" for injured
? Fill: Nature, Description
? Select "No" for attorney
? Click "Save & Create Feature"
? Verify: Feature modal opens
? Create feature with PD coverage
? Verify: Feature created
```

### Third Party Scenario 2: Bicyclist (Injured, No Attorney)
```
? Select type: "Bicyclist"
? Enter name: "Tom Wilson"
? Select "Yes" for injured
? Fill injury details
? Click "Save & Create Feature"
? Verify: Feature modal opens
? Create feature
? Verify: Success
```

### Third Party Scenario 3: Vehicle (No Injury)
```
? Select type: "Vehicle"
? Enter name: "ABC Insurance Corp"
? Fill vehicle details
? Select "No" for injured
? Click "Save & Create Feature"
? Verify: No feature modal opens
? Verify: Third party added
```

---

## ?? Files Modified

### 1. Components/Modals/PassengerModal.razor
- Added injury details validation
- Added attorney details validation
- Added warning alert for injured passengers
- Changed button text to "Save & Create Feature"
- Improved form validation logic

### 2. Components/Modals/ThirdPartyModal.razor
- Added "Bicyclist" party type option
- Added injury details validation for all fields
- Added attorney details validation
- Added warning alert for injured parties
- Changed button text to "Save & Create Feature"
- Improved form validation logic

### 3. Components/Pages/Fnol/FnolStep3_DriverAndInjury.razor
- Added SubClaimModal reference for passengers
- Added CurrentPassengerName variable
- Created AddPassengerAndCreateFeature() method
- Updated RemovePassenger() to also remove features
- Added GetPassengerSubClaims() method
- Feature grid now displays both driver and passenger features

### 4. Components/Pages/Fnol/FnolStep4_ThirdParties.razor (COMPLETELY REWRITTEN)
- Added SubClaimModal reference
- Added ThirdPartySubClaims list
- Added feature grid display
- Created AddThirdPartyAndCreateFeature() method
- Created AddOrUpdateSubClaim() method for features
- Created RemoveFeature() method
- Created RenumberFeatures() method
- Updated RemoveThirdParty() to also remove features
- Added GetThirdPartySubClaims() method

### 5. Components/Pages/Fnol/FnolNew.razor
- Updated GoToStep4() to collect driver features only
- Updated GoToStep5() to collect passenger and third party features
- Updated SubmitClaim() to include all sub-claims

---

## ? Build Status

```
? Build Successful
? All changes compile without errors
? No breaking changes
? Ready for testing and deployment
```

---

## ?? Summary

The workflow is now complete and consistent across all injury parties:

| Party Type | Feature Creation | Coverage | Reserves | Adjuster | Edit/Delete |
|---|---|---|---|---|---|
| Driver | ? Auto | ? | ? | ? | ? |
| Passenger | ? Auto | ? | ? | ? | ? |
| Third Party | ? Auto | ? | ? | ? | ? |

**Status**: ?? **Ready for Production**

