# STANDARDIZED TEMPLATES - QUICK REFERENCE

## ?? AddressTemplate Component

### What It Does:
Displays a reusable address form with optional search functionality.

### Where It's Used:
- ? Passenger Address
- ? Passenger Attorney Address
- ? Third Party Address
- ? Third Party Driver Address
- ? Third Party Attorney Address

### Fields (ALL OPTIONAL):
```
Street Address [with search]
Address Line 2
City | State | Zip Code
```

### How to Use:
```razor
<AddressTemplate 
    @bind-Address1="Object.Address"
    @bind-Address2="Object.Address2"
    @bind-City="Object.City"
    @bind-State="Object.State"
    @bind-ZipCode="Object.ZipCode"
    Label1="Street Address"
    Label2="Address Line 2" />
```

---

## ?? InjuryTemplate Component

### What It Does:
Displays a standardized injury information form.

### Where It's Used:
- ? Passenger Injuries
- ? Third Party Injuries

### Fields (ALL OPTIONAL):
```
Nature of Injury [dropdown]
Date of Medical Treatment [date]
Injury Description [text]
Fatality [checkbox]
Taken to Hospital [checkbox]
  ?? Hospital Name [conditional]
  ?? Hospital Address [conditional]
```

### How to Use:
```razor
<InjuryTemplate 
    InjuryInfo="MyInjuryInfo ?? new()"
    NatureOfInjuries="InjuryList"
    InstanceId="unique_id" />
```

---

## ?? PARTIES USING TEMPLATES

### Insured Vehicle Passenger
```
Card: Address Information
  ?? AddressTemplate (all fields optional)

Card: Attorney Information (if has attorney)
  ?? Attorney Name *
  ?? Firm Name *
  ?? Phone, Email
  ?? AddressTemplate (all fields optional)

Card: Injury Information (if injured)
  ?? InjuryTemplate (all fields optional)
```

### Third Party
```
Card: Address Information
  ?? AddressTemplate (all fields optional)

Card: Contact Information
  ?? Phone
  ?? Email
  ?? FEIN/SS#

IF Vehicle:
  Card: Vehicle Details
    ?? VIN, Year, Make, Model
    
  Card: Driver Information
    ?? Driver Name, DOB, License
    ?? AddressTemplate (driver address)
    ?? Phone, Email, FEIN/SS# (driver contact)

Card: Injury Information (if injured)
  ?? InjuryTemplate (all fields optional)

Card: Attorney Information (if has attorney)
  ?? Attorney Name *
  ?? Firm Name *
  ?? Phone, Email
  ?? AddressTemplate (all fields optional)
```

---

## ? CONSISTENCY CHECKLIST

### All Address Fields Are:
- [ ] Used in AddressTemplate component
- [ ] Optional (no asterisks)
- [ ] Include Street Address with search
- [ ] Include Address2, City, State, Zip
- [ ] Searchable with address lookup

### All Injury Fields Are:
- [ ] Used in InjuryTemplate component
- [ ] Optional (can skip if not injured)
- [ ] Include Nature, Date, Description
- [ ] Include Fatality and Hospital flags
- [ ] Hospital details conditional

### All Attorney Fields Are:
- [ ] Included in modals
- [ ] Name and Firm required if attorney selected
- [ ] Contact fields optional
- [ ] Address fields optional via AddressTemplate

---

## ?? MODAL STRUCTURES

### PassengerModal (modal-xl)
```
?? Passenger Name *
?? Address Information (AddressTemplate)
?? Was Injured? (Radio)
?  ?? Injury Information (InjuryTemplate - if yes)
?? Has Attorney? (Radio)
?  ?? Attorney Information
?     ?? Attorney Name *, Firm Name *
?     ?? Phone, Email
?     ?? Address (AddressTemplate)
?? [Cancel] [Save & Create Feature]
```

### ThirdPartyModal (modal-xl scrollable)
```
?? Party Type *
?? Name *
?? Address Information (AddressTemplate)
?? Contact Information
?  ?? Phone, Email, FEIN/SS#
?? [IF Vehicle Type]
?  ?? Vehicle Details
?  ?? Driver Information
?     ?? Driver Details
?     ?? Driver Address (AddressTemplate)
?     ?? Driver Contact
?? Was Injured? (Radio)
?  ?? Injury Information (InjuryTemplate - if yes)
?? Has Attorney? (Radio)
?  ?? Attorney Information
?     ?? Attorney Name *, Firm Name *
?     ?? Phone, Email
?     ?? Address (AddressTemplate)
?? [Cancel] [Save & Create Feature]
```

---

## ?? TECHNICAL DETAILS

### AddressTemplate.razor
- **File**: `Components/Shared/AddressTemplate.razor`
- **Size**: ~180 lines
- **Dependencies**: IAddressService
- **Features**: Address search, debounced input, auto-fill

### InjuryTemplate.razor
- **File**: `Components/Shared/InjuryTemplate.razor`
- **Size**: ~90 lines
- **Dependencies**: NatureOfInjuries list
- **Features**: Conditional hospital fields, unique instance IDs

### Models Updated
- **AttorneyInfo**: Added Address, Address2, City, State, ZipCode
- **InsuredPassenger**: Added City, State, ZipCode to existing Address
- **ThirdParty**: Added City, State, ZipCode to existing Address
- **DriverInfo**: Already had full address fields

---

## ?? FIELD SUMMARY

### Every Party Now Has:
| Party | Address | Injury | Attorney |
|-------|---------|--------|----------|
| Passenger | ? | ? | ? |
| Third Party | ? | ? | ? |
| TP Driver | ? | - | - |
| TP Attorney | ? | - | - |

### All Fields Are Optional:
- ? Address fields
- ? Injury fields
- ? Attorney fields (except Name & Firm if attorney selected)

---

## ?? IMPLEMENTATION BENEFITS

### For Users:
- Familiar form layout everywhere
- Address search available
- No required fields = flexible data entry
- Consistent injury form

### For Developers:
- DRY principle applied (Don't Repeat Yourself)
- One template = easier maintenance
- Bug fixes apply everywhere
- Easy to add to new modals

### For QA:
- Test once, applies everywhere
- Consistent validation rules
- Easier to catch data inconsistencies

---

## ?? QUICK TEST SCENARIOS

### Scenario 1: Minimal Entry
1. Add Passenger with just name
2. Leave address blank
3. Mark as not injured
4. No attorney
5. Form saves successfully ?

### Scenario 2: Full Entry
1. Add Passenger with all details
2. Search and select address
3. Mark as injured
4. Complete injury form
5. Add attorney with address
6. Form saves successfully ?

### Scenario 3: Third Party Vehicle
1. Add Third Party as Vehicle
2. Complete third party address
3. Complete vehicle details
4. Complete driver details with address
5. Mark injured with injury details
6. Add attorney with address
7. Form saves successfully ?

---

**All Parties** ? Standardized Templates
**All Optional** ? Flexible Entry
**All Consistent** ? Easy Maintenance

---

**Status**: ? READY FOR USE
**Build**: ? SUCCESSFUL
**Quality**: ?????
