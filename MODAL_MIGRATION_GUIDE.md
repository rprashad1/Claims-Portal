# MIGRATION GUIDE - UPDATING MODALS TO USE NEW TEMPLATES

## ?? OVERVIEW

This guide shows how to update existing modals to use the new reusable components:
- PartyInfoForm (replaces individual party fields)
- InjuryInfoForm (replaces individual injury fields)
- AttorneyInfoForm (replaces individual attorney fields)
- AddressSearchInput (replaces manual address entry)

---

## ?? MIGRATION PATTERNS

### Pattern 1: PartyInfoForm Usage

#### Before (PassengerModal example):
```razor
<div class="mb-3">
    <label class="form-label">Name *</label>
    <input type="text" class="form-control" @bind="passenger.Name" />
</div>
<div class="mb-3">
    <label class="form-label">Date of Birth</label>
    <input type="date" class="form-control" @bind="passenger.DateOfBirth" />
</div>
<!-- ... more fields ... -->
```

#### After (Using PartyInfoForm):
```razor
<PartyInfoForm @bind-PartyData="partyData"
              Title="Passenger Information"
              IncludeFeinSsNumber="true" />
```

**Then map to model:**
```csharp
passenger.Name = partyData.Name;
passenger.Address = partyData.Address;
passenger.PhoneNumber = partyData.PhoneNumber;
// ... etc
```

---

### Pattern 2: InjuryInfoForm Usage

#### Before (In PassengerModal):
```razor
<div class="mb-3">
    <label class="form-label">Nature of Injury *</label>
    <input type="text" class="form-control" @bind="passenger.InjuryInfo.NatureOfInjury" />
</div>
<div class="mb-3">
    <label class="form-label">First Medical Treatment Date</label>
    <input type="date" class="form-control" @bind="passenger.InjuryInfo.FirstMedicalTreatmentDate" />
</div>
<!-- ... more fields ... -->
```

#### After (Using InjuryInfoForm):
```razor
@if (passenger.WasInjured && passenger.InjuryInfo != null)
{
    <InjuryInfoForm @bind-InjuryData="injuryRecord"
                   Title="Passenger Injury Details"
                   InjuryTypes="InjuryTypes" />
}
```

**Then map back:**
```csharp
passenger.InjuryInfo = new InjuryInfo
{
    NatureOfInjury = injuryRecord.InjuryType,
    SeverityLevel = injuryRecord.SeverityLevel,
    FirstMedicalTreatmentDate = injuryRecord.FirstMedicalTreatmentDate,
    // ... etc
};
```

---

### Pattern 3: AttorneyInfoForm Usage

#### Before (In PassengerModal):
```razor
<div class="mb-3">
    <label class="form-label">Attorney Name *</label>
    <input type="text" class="form-control" @bind="passenger.AttorneyInfo.Name" />
</div>
<div class="mb-3">
    <label class="form-label">Firm Name *</label>
    <input type="text" class="form-control" @bind="passenger.AttorneyInfo.FirmName" />
</div>
<!-- ... more fields ... -->
```

#### After (Using AttorneyInfoForm):
```razor
@if (passenger.HasAttorney && passenger.AttorneyInfo != null)
{
    <AttorneyInfoForm @bind-AttorneyData="attorneyData"
                     Title="Passenger Attorney Information" />
}
```

**Then map back:**
```csharp
passenger.AttorneyInfo = new AttorneyInfo
{
    Name = attorneyData.Name,
    FirmName = attorneyData.FirmName,
    Address = attorneyData.Address,
    // ... etc
};
```

---

### Pattern 4: AddressSearchInput Usage

#### Before (Manual entry):
```razor
<div class="mb-3">
    <label class="form-label">Address *</label>
    <input type="text" class="form-control" @bind="address" />
</div>
<div class="mb-3">
    <label class="form-label">City *</label>
    <input type="text" class="form-control" @bind="city" />
</div>
<div class="mb-3">
    <label class="form-label">State *</label>
    <input type="text" class="form-control" @bind="state" />
</div>
<div class="mb-3">
    <label class="form-label">Zip Code *</label>
    <input type="text" class="form-control" @bind="zip" />
</div>
```

#### After (Using AddressSearchInput):
```razor
<AddressSearchInput @bind-Address1="address"
                   @bind-City="city"
                   @bind-State="state"
                   @bind-ZipCode="zip"
                   Label1="Street Address"
                   IsRequired="true" />
```

---

## ??? STEP-BY-STEP MIGRATION

### Step 1: Update PassengerModal

#### 1.1 Add using statements
```razor
@using ClaimsPortal.Models
```

#### 1.2 Create Party and InjuryRecord variables
```csharp
private Party partyData = new();
private InjuryRecord injuryRecord = new();
private Attorney attorneyData = new();
```

#### 1.3 Replace passenger field entry section
```razor
<!-- Before: Multiple individual input fields -->
<!-- After: Single component -->
<PartyInfoForm @bind-PartyData="partyData"
              Title="Passenger Information"
              IncludeFeinSsNumber="true" />
```

#### 1.4 Replace injury field entry section
```razor
@if (passenger.WasInjured && passenger.InjuryInfo != null)
{
    <InjuryInfoForm @bind-InjuryData="injuryRecord"
                   Title="Passenger Injury Details"
                   InjuryTypes="NatureOfInjuries" />
}
```

#### 1.5 Replace attorney field entry section
```razor
@if (passenger.HasAttorney && passenger.AttorneyInfo != null)
{
    <AttorneyInfoForm @bind-AttorneyData="attorneyData"
                     Title="Passenger Attorney Information" />
}
```

#### 1.6 Update OnAdd method to map data
```csharp
private async Task OnAdd()
{
    if (IsFormValid())
    {
        // Map PartyData to Passenger
        passenger.Name = partyData.Name;
        passenger.Address = partyData.Address;
        passenger.PhoneNumber = partyData.PhoneNumber;
        passenger.Email = partyData.Email;
        passenger.FeinSsNumber = partyData.FeinSsNumber;
        
        // Map InjuryRecord to InjuryInfo (if injured)
        if (passenger.WasInjured && injuryRecord != null)
        {
            passenger.InjuryInfo = new InjuryInfo
            {
                NatureOfInjury = injuryRecord.InjuryType,
                SeverityLevel = injuryRecord.SeverityLevel,
                FirstMedicalTreatmentDate = injuryRecord.FirstMedicalTreatmentDate,
                IsFatality = injuryRecord.IsFatality,
                WasTakenToHospital = injuryRecord.WasTakenToHospital,
                HospitalName = injuryRecord.HospitalName,
                HospitalAddress = injuryRecord.HospitalAddress,
                InjuryDescription = injuryRecord.InjuryDescription
            };
        }
        
        // Map Attorney data (if has attorney)
        if (passenger.HasAttorney && attorneyData != null)
        {
            passenger.AttorneyInfo = new AttorneyInfo
            {
                Name = attorneyData.Name,
                FirmName = attorneyData.FirmName,
                Address = attorneyData.Address,
                PhoneNumber = attorneyData.PhoneNumber,
                Email = attorneyData.Email
            };
        }
        
        await OnPassengerAdded.InvokeAsync(passenger);
        await JS.InvokeVoidAsync("HideModal", "passengerModal");
    }
}
```

---

### Step 2: Update ThirdPartyModal

#### 2.1 Create Party variables
```csharp
private Party partyData = new();
private Attorney attorneyData = new();
```

#### 2.2 Replace name/address entry
```razor
<PartyInfoForm @bind-PartyData="partyData"
              Title="Third Party Information"
              IncludeFeinSsNumber="true" />
```

#### 2.3 For Vehicle-specific info (Driver)
```razor
<!-- Keep VehicleInfo as-is -->
<!-- Add driver address from party -->
```

#### 2.4 Update mapping in OnAdd
```csharp
thirdParty.Name = partyData.Name;
thirdParty.Address = partyData.Address;
thirdParty.Address2 = partyData.Address2;
thirdParty.PhoneNumber = partyData.PhoneNumber;
thirdParty.Email = partyData.Email;
thirdParty.FeinSsNumber = partyData.FeinSsNumber;
```

---

### Step 3: Data Mapping Helper

Create a helper class to simplify mapping:

```csharp
public static class DataMappingHelper
{
    public static void MapPartyToPassenger(Party source, InsuredPassenger target)
    {
        target.Name = source.Name;
        target.Address = source.Address;
        target.Address2 = source.Address2;
        target.PhoneNumber = source.PhoneNumber;
        target.Email = source.Email;
        target.FeinSsNumber = source.FeinSsNumber;
    }
    
    public static void MapInjuryRecordToInjuryInfo(InjuryRecord source, InjuryInfo target)
    {
        target.NatureOfInjury = source.InjuryType;
        target.FirstMedicalTreatmentDate = source.FirstMedicalTreatmentDate;
        target.IsFatality = source.IsFatality;
        target.WasTakenToHospital = source.WasTakenToHospital;
        target.HospitalName = source.HospitalName;
        target.HospitalAddress = source.HospitalAddress;
        target.InjuryDescription = source.InjuryDescription;
    }
    
    public static void MapAttorneyToAttorneyInfo(Attorney source, AttorneyInfo target)
    {
        target.Name = source.Name;
        target.FirmName = source.FirmName;
        target.PhoneNumber = source.PhoneNumber;
        target.Email = source.Email;
        target.Address = source.Address;
    }
}
```

Then use in modal:
```csharp
DataMappingHelper.MapPartyToPassenger(partyData, passenger);
DataMappingHelper.MapInjuryRecordToInjuryInfo(injuryRecord, passenger.InjuryInfo);
DataMappingHelper.MapAttorneyToAttorneyInfo(attorneyData, passenger.AttorneyInfo);
```

---

## ?? MIGRATION CHECKLIST

### PassengerModal
- [ ] Add using statements
- [ ] Create Party, InjuryRecord, Attorney variables
- [ ] Replace name field with PartyInfoForm
- [ ] Replace address fields with PartyInfoForm
- [ ] Replace phone/email with PartyInfoForm
- [ ] Replace FeinSsNumber field with PartyInfoForm
- [ ] Replace injury fields with InjuryInfoForm
- [ ] Replace attorney fields with AttorneyInfoForm
- [ ] Update OnAdd method mapping
- [ ] Test modal functionality
- [ ] Test data persistence

### ThirdPartyModal
- [ ] Add using statements
- [ ] Create Party, Attorney variables
- [ ] Replace name/address with PartyInfoForm
- [ ] Replace attorney fields with AttorneyInfoForm
- [ ] Keep VehicleInfo as-is
- [ ] Update OnAdd method mapping
- [ ] Test modal functionality

### Other Modals
- [ ] WitnessModal (if needs address)
- [ ] PropertyOwner fields (if needs address)
- [ ] Any other party entry points

---

## ? BENEFITS AFTER MIGRATION

? **Code Reduction** - Less repetitive field definitions
? **Consistency** - Same field definitions across all modals
? **Maintainability** - Update component once, affects everywhere
? **Better UX** - Address autocomplete in all modals
? **Type Safety** - Use Party, InjuryRecord, Attorney classes
? **Reusability** - Components usable in any new modals

---

**Migration Path**: CLEAR & SYSTEMATIC
**Effort**: MODERATE (1-2 hours per modal)
**Value**: HIGH (Long-term maintainability)

