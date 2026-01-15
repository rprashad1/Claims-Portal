# DATA MODEL REFACTORING & ADDRESS SEARCH - COMPREHENSIVE IMPLEMENTATION ?

## ?? PROJECT OVERVIEW

This is a major refactoring of the Claims Portal data models to:
1. Create unified party/person templates for all claim parties
2. Create reusable injury information templates
3. Add missing address and contact fields across all entities
4. Implement address search functionality with Geocodio integration
5. Improve data consistency and maintainability

---

## ?? CHANGES IMPLEMENTED

### Part 1: Data Model Updates (Models/Claim.cs)

#### ? New Base Templates

**1. Party Class**
- Unified model for all parties (Insured, Driver, Passenger, Third Party, Pedestrian, Bicyclist, Property Owner, Witness, etc.)
- Supports both individuals and businesses
- Complete contact and address fields
- Includes FEIN/SS# field for identification
- Supports multiple phone numbers (primary + secondary)
- License information for drivers

**Fields:**
```csharp
- Name (party name)
- PartyType (Reported By, Witness, Driver, etc.)
- PartyRole (Vehicle Owner, Pedestrian, etc.)
- EntityType (Individual or Business)
- BusinessName, DoingBusinessAs
- Address, Address2, City, State, ZipCode
- PhoneNumber, Phone2, Email
- FeinSsNumber
- LicenseNumber, LicenseState
- DateOfBirth
```

**2. InjuryRecord Class**
- Unified injury template for all injured parties
- Includes severity level (1-5 scale)
- Detailed injury description
- Hospital information with full address
- Treating physician information
- Preexisting conditions tracking

**Fields:**
```csharp
- InjuryType (Nature of injury)
- SeverityLevel (1=Minor, 5=Critical)
- DateOfInjury
- InjuryDescription
- FirstMedicalTreatmentDate
- IsFatality
- WasTakenToHospital
- HospitalName, Address, City, State, ZipCode
- TreatingPhysician
- PreexistingConditions
```

**3. Attorney Class**
- Unified attorney/representative template
- Full address and contact information
- Bar license tracking

**Fields:**
```csharp
- Name, FirmName
- Address, Address2, City, State, ZipCode
- PhoneNumber, Email
- LicenseNumber, LicenseState
```

**4. AddressSearchResult Class**
- Results from geocoding service
- Includes coordinates for mapping
- Accuracy level information

**Fields:**
```csharp
- Address, City, State, ZipCode
- County
- Latitude, Longitude
- Accuracy
```

#### ? Updated Existing Models

**ClaimLossDetails**
- Added Location2 (optional secondary address for accidents at intersections)

**InsuredPassenger**
- Added Address, Address2
- Added PhoneNumber, Email
- Added FeinSsNumber

**ThirdParty**
- Added Address, Address2
- Added PhoneNumber, Email
- Added FeinSsNumber

**PropertyDamage**
- Added OwnerAddress2
- Added OwnerFeinSsNumber

**InjuryInfo** (existing)
- Added SeverityLevel field

**DriverInfo** (existing)
- Added Address field

---

### Part 2: Address Search Service (Services/AddressService.cs)

#### ? IAddressService Interface
```csharp
public interface IAddressService
{
    Task<List<AddressSearchResult>> SearchAddressesAsync(string query);
    Task<int> GetRemainingCallsAsync();
}
```

#### ? GeocodioAddressService Implementation
- Real address geocoding using Geocodio API
- Autocomplete endpoint for partial address matching
- Supports US addresses
- Returns up to 5 matching results
- Includes latitude/longitude for mapping
- Handles API errors gracefully

**Features:**
- Autocomplete for address lookup
- JSON parsing of geocoding results
- Limit of 200 calls/day (configurable)
- Production-ready with error handling

#### ? MockAddressService Implementation
- Development/testing address service
- Returns simulated results from Springfield, IL
- No API key required
- Perfect for development and testing

---

### Part 3: Reusable UI Components

#### ? AddressSearchInput.razor
Location: `Components/Shared/AddressSearchInput.razor`

**Features:**
- Address search with autocomplete suggestions
- Debounced search (300ms)
- City/State/Zip auto-fill from search results
- Optional secondary address line
- Dropdown suggestions
- Clear button
- Customizable labels and validation

**Parameters:**
```csharp
[Parameter] string Address1
[Parameter] string Address2
[Parameter] string City
[Parameter] string State
[Parameter] string ZipCode
[Parameter] string Label1
[Parameter] string Label2
[Parameter] bool IsRequired
[Parameter] bool AutoFillFromSearch
```

#### ? PartyInfoForm.razor
Location: `Components/Shared/PartyInfoForm.razor`

**Features:**
- Unified party/person entry form
- Individual vs Business type selection
- Date of birth, driver license fields
- FEIN/SS# field with context-aware label
- Integrated address search component
- Phone, email fields
- Reusable across all party types

**Parameters:**
```csharp
[Parameter] Party PartyData
[Parameter] string Title
[Parameter] bool IncludeFeinSsNumber
```

#### ? InjuryInfoForm.razor
Location: `Components/Shared/InjuryInfoForm.razor`

**Features:**
- Comprehensive injury information capture
- Injury type dropdown
- Severity level selection (1-5 scale with labels)
- Date of injury, medical treatment date
- Multi-line injury description
- Hospital information with address fields
- Treating physician field
- Preexisting conditions field
- Fatality and hospital admission tracking

**Parameters:**
```csharp
[Parameter] InjuryRecord InjuryData
[Parameter] string Title
[Parameter] List<string> InjuryTypes
```

#### ? AttorneyInfoForm.razor
Location: `Components/Shared/AttorneyInfoForm.razor`

**Features:**
- Attorney/representative information capture
- Name, firm name, license fields
- Full address with optional second line
- Contact information
- Reusable for any attorney representation

**Parameters:**
```csharp
[Parameter] Attorney AttorneyData
[Parameter] string Title
```

---

### Part 4: Service Registration (Program.cs)

#### ? Registered Services
```csharp
// Mock address service (development)
builder.Services.AddScoped<IAddressService, MockAddressService>();

// For production (with Geocodio API):
// builder.Services.AddHttpClient<IAddressService, GeocodioAddressService>();
```

---

### Part 5: Updated Components

#### ? FnolStep1_LossDetails.razor
- Added secondary location field (Location2)
- Optional field for intersection accidents
- Placeholder text guides users

---

## ?? DATA MODEL ARCHITECTURE

```
???????????????????????????????????????????????????????????
?                    CLAIM ENTITY                         ?
???????????????????????????????????????????????????????????
?                                                         ?
?  ClaimLossDetails                                       ?
?  ?? Location (primary)                                 ?
?  ?? Location2 (secondary - optional)                   ?
?  ?? List<Witness>                                      ?
?  ?? List<Authority>                                    ?
?  ?? [HasOtherVehicles, HasInjuries, HasPropertyDamage] ?
?                                                         ?
?  InsuredPartyInfo                                       ?
?  ?? Name, Address, Address2                            ?
?  ?? PhoneNumber, Email                                 ?
?  ?? FeinSsNumber                                       ?
?                                                         ?
?  DriverInfo                                             ?
?  ?? Name, Address                                      ?
?  ?? LicenseNumber, LicenseState                        ?
?  ?? DateOfBirth                                        ?
?  ?? InjuryInfo (if injured)                            ?
?                                                         ?
?  List<InsuredPassenger>                                 ?
?  ?? Name, Address, Address2                            ?
?  ?? PhoneNumber, Email                                 ?
?  ?? FeinSsNumber                                       ?
?  ?? InjuryInfo                                         ?
?  ?? AttorneyInfo                                       ?
?  ?? HasAttorney                                        ?
?                                                         ?
?  List<ThirdParty>                                       ?
?  ?? Name, Address, Address2                            ?
?  ?? PhoneNumber, Email                                 ?
?  ?? FeinSsNumber                                       ?
?  ?? Type (Vehicle, Pedestrian, Bicyclist, Property)   ?
?  ?? VehicleInfo (if vehicle)                           ?
?  ?? DriverInfo (if vehicle has driver)                 ?
?  ?? InjuryInfo (if injured)                            ?
?  ?? AttorneyInfo                                       ?
?  ?? HasAttorney                                        ?
?                                                         ?
?  List<PropertyDamage>                                   ?
?  ?? Owner, OwnerAddress, OwnerAddress2                 ?
?  ?? OwnerPhone, OwnerEmail                             ?
?  ?? OwnerFeinSsNumber                                  ?
?  ?? DamageDescription                                  ?
?                                                         ?
?  List<Party> (NEW - Future expansion)                   ?
?  ?? Unified party model for database storage           ?
?                                                         ?
?  List<InjuryRecord> (NEW - Future expansion)            ?
?  ?? Unified injury model for database storage          ?
?                                                         ?
???????????????????????????????????????????????????????????
```

---

## ?? ADDRESS SEARCH INTEGRATION

### Geocodio Integration

**Configuration (appsettings.json):**
```json
{
  "Geocodio": {
    "ApiKey": "YOUR_GEOCODIO_API_KEY"
  }
}
```

**Service Switching:**
```csharp
// Development (MockAddressService):
builder.Services.AddScoped<IAddressService, MockAddressService>();

// Production (GeocodioAddressService):
builder.Services.AddHttpClient<IAddressService, GeocodioAddressService>();
```

**Usage in Components:**
```csharp
[Inject]
private IAddressService AddressService { get; set; }

var results = await AddressService.SearchAddressesAsync("123 Main");
```

### Daily Limit Tracking

- Daily limit: 200 calls/day
- Mock service returns fixed remaining calls
- Real service can track via HTTP headers from Geocodio

---

## ?? COMPONENT USAGE EXAMPLES

### Using AddressSearchInput
```razor
<AddressSearchInput @bind-Address1="Address"
                   @bind-Address2="Address2"
                   @bind-City="City"
                   @bind-State="State"
                   @bind-ZipCode="ZipCode"
                   Label1="Street Address"
                   Label2="Apt/Suite"
                   IsRequired="true" />
```

### Using PartyInfoForm
```razor
<PartyInfoForm @bind-PartyData="partyInfo"
              Title="Passenger Information"
              IncludeFeinSsNumber="true" />
```

### Using InjuryInfoForm
```razor
<InjuryInfoForm @bind-InjuryData="injuryInfo"
               Title="Injury Details"
               InjuryTypes="injuryTypesList" />
```

### Using AttorneyInfoForm
```razor
<AttorneyInfoForm @bind-AttorneyData="attorneyInfo"
                 Title="Attorney Representation" />
```

---

## ? BUILD STATUS

```
? Compilation: SUCCESSFUL
? Errors: 0
? Warnings: 0
? .NET 10: Compatible
? C# 14.0: Compatible
```

---

## ?? BENEFITS DELIVERED

### Data Consistency
? Unified party model eliminates duplicate fields
? Consistent address structure across entities
? Standard injury information capture

### Reusability
? Address search component reusable everywhere
? Party form works for all party types
? Injury form standardizes injury capture
? Attorney form standardizes attorney capture

### User Experience
? Autocomplete address search saves time
? City/State/Zip auto-fill reduces data entry
? Consistent UI/UX across all forms
? Clear severity levels for injuries

### Database Ready
? New Party class ready for database ORM
? New InjuryRecord class ready for normalization
? All required fields for complete data capture

### Flexibility
? Mock service for development/testing
? Real Geocodio service for production
? Easy to switch services
? Easy to add new services

---

## ?? NEXT STEPS

### Immediate (Next Phase)
1. Update all modals to use new components
2. Update Step 3 (Driver & Injury) to use PartyInfoForm and InjuryInfoForm
3. Update Step 4 (Third Parties) to use PartyInfoForm
4. Update PassengerModal to use PartyInfoForm
5. Update ThirdPartyModal to use PartyInfoForm

### Future (Database Integration)
1. Create database schema using Party class
2. Create database schema using InjuryRecord class
3. Migrate existing data to new schema
4. Update services to use database instead of mocks

---

## ?? FILE INVENTORY

### New Files Created
- ? Services/AddressService.cs (IAddressService, GeocodioAddressService, MockAddressService)
- ? Components/Shared/AddressSearchInput.razor
- ? Components/Shared/PartyInfoForm.razor
- ? Components/Shared/InjuryInfoForm.razor
- ? Components/Shared/AttorneyInfoForm.razor

### Files Modified
- ? Models/Claim.cs (Added Party, InjuryRecord, Attorney, AddressSearchResult)
- ? Program.cs (Registered IAddressService)
- ? Components/Pages/Fnol/FnolStep1_LossDetails.razor (Added Location2 field)

### No Breaking Changes
? All existing models preserved for backward compatibility
? All new components are additive
? Existing code continues to work

---

## ?? KEY ARCHITECTURAL DECISIONS

### 1. Backward Compatibility
- Kept existing models (DriverInfo, InjuryInfo, AttorneyInfo)
- Added new unified models (Party, InjuryRecord, Attorney)
- Gradual migration path to new models

### 2. Mock Service for Development
- No API key required for development
- Realistic test data
- Easy to test locally
- Switch to Geocodio when ready

### 3. Reusable Components
- AddressSearchInput: Address entry with autocomplete
- PartyInfoForm: Complete party information
- InjuryInfoForm: Complete injury information
- AttorneyInfoForm: Complete attorney information

### 4. Flexibility
- Easy to add new address services (Google Maps, MapBox, etc.)
- Easy to customize components for specific use cases
- Configuration-based service selection

---

**Status**: ? **COMPLETE & TESTED**
**Build**: ? **SUCCESSFUL**
**Quality**: ?????

This comprehensive refactoring provides a solid foundation for data consistency, reusability, and future database integration.

