# Data Model - Complete Picture

## What Gets Saved and When

### Driver Information (Immediate)
```csharp
DriverInfo {
    Name: string                    // "Insured", "John Doe", etc.
    LicenseNumber: string          // Optional if insured
    LicenseState: string           // Optional if insured
    IsListed: bool                 // true/false based on driver type
}
```
**Saved**: Captured as user types  
**Cleared**: When driver type changes  

---

### Injury Information (When Driver Injured)
```csharp
InjuryInfo {
    NatureOfInjury: string         // "Whiplash", "Fracture", etc.
    FirstMedicalTreatmentDate: DateTime
    IsFatality: bool
    WasTakenToHospital: bool
    HospitalName: string
    HospitalAddress: string
    InjuryDescription: string
}
```
**Saved**: Captured as user types  
**Cleared**: When user selects "No" for injured  

---

### Attorney Information (If Attorney Exists)
```csharp
AttorneyInfo {
    Name: string
    FirmName: string
    PhoneNumber: string
    Email: string
    Address: string
}
```
**Saved**: Captured as user types  
**Cleared**: When user selects "No" for attorney  

---

### Feature / Sub-Claim (After Save Button)
```csharp
SubClaim {
    Id: int                        // Auto-generated
    FeatureNumber: string          // "01", "02", etc.
    Coverage: string              // "BI", "PD", "PIP", "APIP", "UM"
    CoverageLimits: string         // "100/300", "50,000", etc.
    ClaimantName: string           // Driver name
    ExpenseReserve: decimal        // Dollar amount
    IndemnityReserve: decimal      // Dollar amount
    AssignedAdjusterId: string     // "raj", "edwin", etc.
    AssignedAdjusterName: string   // "Raj", "Edwin", etc.
    Status: string                 // "Open"
    CreatedDate: DateTime
    ClaimType: string              // "Driver", "Passenger", "ThirdParty"
}
```
**Saved**: ONLY when feature creation modal submitted  
**Location**: DriverSubClaims list  
**Displayed**: Feature grid at bottom of section  

---

## Complete Step 3 Data Structure

```csharp
// After step 3 is complete and validated:

Claim {
    Driver: DriverInfo {
        Name: "Insured",
        LicenseNumber: "",
        LicenseState: "",
        IsListed: true
    },
    
    DriverInjury: InjuryInfo | null {
        NatureOfInjury: "Whiplash",
        FirstMedicalTreatmentDate: 2024-01-15,
        IsFatality: false,
        WasTakenToHospital: true,
        HospitalName: "City Hospital",
        HospitalAddress: "123 Main St",
        InjuryDescription: "Neck and back pain"
    },
    
    DriverAttorney: AttorneyInfo | null {
        Name: "Jane Smith",
        FirmName: "Smith & Associates",
        PhoneNumber: "555-0123",
        Email: "jane@smithlaw.com",
        Address: "456 Law Ave"
    },
    
    SubClaims: List<SubClaim> [
        {
            Id: 12345,
            FeatureNumber: "01",
            Coverage: "BI",
            CoverageLimits: "100/300",
            ClaimantName: "Insured",
            ExpenseReserve: 5000.00,
            IndemnityReserve: 25000.00,
            AssignedAdjusterId: "raj",
            AssignedAdjusterName: "Raj",
            Status: "Open",
            CreatedDate: 2024-01-20,
            ClaimType: "Driver"
        }
    ],
    
    Passengers: List<InsuredPassenger> [],  // Populated in next section
    ThirdParties: List<ThirdParty> []       // Populated in next section
}
```

---

## State Management

### Component State Variables

```csharp
// Driver info
private string DriverType = "insured";              // Reactive
private DriverInfo Driver = new();                  // Reactive
private bool DriverSaved = false;                   // Controls next step

// Injury info
private bool IsDriverInjured = false;               // Reactive
private InjuryInfo DriverInjury = new();            // Reactive

// Attorney info
private bool HasAttorney = false;                   // Reactive
private AttorneyInfo DriverAttorney = new();        // Reactive

// Sub-claims
private List<SubClaim> DriverSubClaims = [];        // Grid display
private int FeatureCounter = 0;                     // Auto-numbering

// Passengers & Lookups
private List<InsuredPassenger> Passengers = [];
private List<string> NatureOfInjuries = [];
```

### Flag Behavior

**`IsNextDisabled`** - Controls navigation to Step 4
```csharp
private bool IsNextDisabled => !DriverSaved || (Driver.Name == string.Empty);

// Step 4 only becomes available when:
// 1. DriverSaved == true AND
// 2. Driver.Name is not empty
```

**`DriverSaved`** - Set to true when:
- Driver has no injury AND user clicks "Save Driver & Create Feature"
- OR feature is created successfully in modal

**`CanSaveDriver()`** - Validates:
- Driver name is filled
- If injured: nature of injury, medical date, description are filled
- If attorney: attorney name and firm name are filled

---

## Validation Rules

### Required Fields by Scenario

**Scenario 1: No Injury**
```
? Driver Name (varies by type)
? Can save immediately
```

**Scenario 2: Injured, No Attorney**
```
? Driver Name
? Nature of Injury
? Date of First Medical Treatment
? Injury Description
? Coverage Type (in feature modal)
? Expense Reserve (> 0 in feature modal)
? Indemnity Reserve (> 0 in feature modal)
? Assigned Adjuster (in feature modal)
? Feature must be created
```

**Scenario 3: Injured with Attorney**
```
? Driver Name
? Nature of Injury
? Date of First Medical Treatment
? Injury Description
? Attorney Name
? Attorney Firm Name
? Coverage Type (in feature modal)
? Expense Reserve (> 0 in feature modal)
? Indemnity Reserve (> 0 in feature modal)
? Assigned Adjuster (in feature modal)
? Feature must be created
```

---

## Data Flow Diagram

```
???????????????????????????????????????????????????????????
?              USER INTERACTION LAYER                      ?
???????????????????????????????????????????????????????????
              ?
    ???????????????????????????
    ? Driver Type Selection   ? ? DriverType
    ???????????????????????????
              ?
    ???????????????????????????
    ? Driver Details Entry    ? ? Driver (DriverInfo)
    ???????????????????????????
              ?
    ???????????????????????????
    ? Injury Yes/No?          ? ? IsDriverInjured
    ???????????????????????????
              ?
    ????????????????????????????????????????
    ? IF Injured:                          ?
    ? Capture Injury Details           ? DriverInjury
    ? Capture Attorney Details         ? DriverAttorney
    ? Validate All Fields              ? CanSaveDriver()
    ????????????????????????????????????????
              ?
    ????????????????????????????????????????
    ? SAVE DRIVER & CREATE FEATURE Button  ?
    ? Opens SubClaimModal                  ?
    ????????????????????????????????????????
              ?
    ????????????????????????????????????????
    ? Feature Modal:                       ?
    ? • Coverage Selection                 ?
    ? • Reserve Entry                      ?
    ? • Adjuster Assignment                ?
    ? • Validation & Summary               ?
    ????????????????????????????????????????
              ?
    ????????????????????????????????????????
    ? Create Feature Button                ?
    ? Adds to DriverSubClaims list     ? SubClaim
    ? Sets DriverSaved = true          ? IsNextDisabled
    ? Modal closes                        ?
    ????????????????????????????????????????
              ?
    ????????????????????????????????????????
    ? Feature Grid Displays:               ?
    ? • All created features               ?
    ? • Edit/Delete options                ?
    ? • Running feature numbers            ?
    ????????????????????????????????????????
              ?
    ????????????????????????????????????????
    ? Next Button Enabled                  ?
    ? User can proceed to Step 4           ?
    ????????????????????????????????????????
```

---

## How Data Flows to Next Steps

### After Step 3 Completion

```csharp
private async Task GoToStep4()
{
    if (step3Component != null && await step3Component.ValidateAsync())
    {
        // Get all driver data
        CurrentClaim.Driver = step3Component.GetDriver();
        CurrentClaim.DriverInjury = step3Component.GetDriverInjury();
        CurrentClaim.DriverAttorney = step3Component.GetDriverAttorney();
        CurrentClaim.Passengers = step3Component.GetPassengers();
        
        // Get all sub-claims created for driver
        var driverSubClaims = step3Component.GetDriverSubClaims();
        CurrentClaim.SubClaims.AddRange(driverSubClaims);
        
        // Move to next step
        CurrentStep = 4;
    }
}
```

**Result in CurrentClaim**:
```csharp
Claim {
    Driver: DriverInfo,              // ? Populated
    DriverInjury: InjuryInfo | null, // ? Populated or null
    DriverAttorney: AttorneyInfo | null, // ? Populated or null
    SubClaims: [                     // ? Populated with driver features
        SubClaim { FeatureNumber: "01", ... }
    ],
    Passengers: [],                  // Empty, filled in Step 3
    ThirdParties: []                 // Empty, filled in Step 4
}
```

---

## Summary

? **Clear ownership**: Each data piece knows which section/modal handles it  
? **Atomic saves**: Feature created only when complete  
? **Validation enforced**: Can't proceed without required data  
? **Status tracking**: DriverSaved flag prevents incomplete progression  
? **Flexible editing**: Features can be edited/deleted before next step  

