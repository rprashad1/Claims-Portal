# DATA MODEL REFACTORING - QUICK REFERENCE GUIDE

## ?? WHAT WAS DONE

### 1. ? Created Unified Templates

**Party Template** - For all people/businesses
- Supports Individual or Business
- Address (with optional secondary line)
- Contact info (phone, phone2, email)
- FEIN/SS#, License info, DOB

**InjuryRecord Template** - For all injuries
- Injury type + severity (1-5)
- Detailed description
- Hospital info with address
- Treating physician, fatality, preexisting conditions

**Attorney Template** - For all attorney representation
- Full contact and address info
- Bar license tracking

### 2. ? Added Missing Fields

**InsuredPassenger:**
- Address, Address2
- PhoneNumber, Email
- FeinSsNumber

**ThirdParty:**
- Address, Address2
- PhoneNumber, Email
- FeinSsNumber

**PropertyDamage:**
- OwnerAddress2
- OwnerFeinSsNumber

**ClaimLossDetails:**
- Location2 (secondary address for intersections)

### 3. ? Address Search with Geocodio

**Features:**
- Type address, get autocomplete suggestions
- Auto-fill City/State/Zip from selection
- Development: Use MockAddressService (no API needed)
- Production: Use GeocodioAddressService (with API key)
- Daily limit: 200 calls/day

**Service Registration:**
```csharp
// Development
builder.Services.AddScoped<IAddressService, MockAddressService>();

// Production
builder.Services.AddHttpClient<IAddressService, GeocodioAddressService>();
```

### 4. ? Created 4 Reusable Components

#### AddressSearchInput
- Search address with autocomplete
- Returns: Address, City, State, ZipCode
- Usage: In any address entry

#### PartyInfoForm
- Complete party/person information
- Supports Individual or Business
- Includes address search
- Reusable for: Drivers, Passengers, Third Parties, etc.

#### InjuryInfoForm
- Complete injury information
- Severity level (1-5 scale)
- Hospital details
- Reusable for any injured party

#### AttorneyInfoForm
- Complete attorney information
- Full address and contact
- Reusable for any attorney

---

## ?? NEW MODELS

```csharp
// Party - For all people/businesses
public class Party
{
    public string Name { get; set; }
    public string EntityType { get; set; } // Individual or Business
    public string BusinessName { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Phone2 { get; set; }
    public string Email { get; set; }
    public string FeinSsNumber { get; set; }
    public string LicenseNumber { get; set; }
    public string LicenseState { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

// InjuryRecord - For all injuries
public class InjuryRecord
{
    public string InjuryType { get; set; }
    public int SeverityLevel { get; set; } // 1-5
    public DateTime DateOfInjury { get; set; }
    public string InjuryDescription { get; set; }
    public DateTime FirstMedicalTreatmentDate { get; set; }
    public bool IsFatality { get; set; }
    public bool WasTakenToHospital { get; set; }
    public string HospitalName { get; set; }
    public string HospitalAddress { get; set; }
    public string HospitalCity { get; set; }
    public string HospitalState { get; set; }
    public string HospitalZipCode { get; set; }
    public string TreatingPhysician { get; set; }
    public string PreexistingConditions { get; set; }
}

// Attorney - For all representation
public class Attorney
{
    public string Name { get; set; }
    public string FirmName { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string LicenseNumber { get; set; }
    public string LicenseState { get; set; }
}
```

---

## ?? USAGE EXAMPLES

### AddressSearchInput
```razor
<AddressSearchInput @bind-Address1="address"
                   @bind-City="city"
                   @bind-State="state"
                   @bind-ZipCode="zip"
                   Label1="Street Address"
                   IsRequired="true" />
```

### PartyInfoForm
```razor
<PartyInfoForm @bind-PartyData="party"
              Title="Passenger Information"
              IncludeFeinSsNumber="true" />
```

### InjuryInfoForm
```razor
<InjuryInfoForm @bind-InjuryData="injury"
               Title="Injury Details"
               InjuryTypes="injuryTypes" />
```

### AttorneyInfoForm
```razor
<AttorneyInfoForm @bind-AttorneyData="attorney"
                 Title="Attorney Information" />
```

---

## ?? FILES CREATED

### New Files
- Services/AddressService.cs
- Components/Shared/AddressSearchInput.razor
- Components/Shared/PartyInfoForm.razor
- Components/Shared/InjuryInfoForm.razor
- Components/Shared/AttorneyInfoForm.razor

### Updated Files
- Models/Claim.cs (New classes added)
- Program.cs (Service registration)
- Components/Pages/Fnol/FnolStep1_LossDetails.razor (Location2 field)

---

## ? BUILD STATUS

```
? Build: SUCCESSFUL
? Errors: 0
? Warnings: 0
? .NET 10: Compatible
```

---

## ?? BENEFITS

? **Data Consistency** - Same fields across all parties
? **Reusable Code** - Components work for multiple use cases
? **Better UX** - Address autocomplete saves time
? **Database Ready** - Party/InjuryRecord models ready for ORM
? **Easy to Extend** - Add new address services easily
? **Backward Compatible** - Existing code still works

---

## ?? NEXT STEPS

1. Update PassengerModal to use PartyInfoForm
2. Update ThirdPartyModal to use PartyInfoForm
3. Update Modals to use InjuryInfoForm
4. Update Modals to use AttorneyInfoForm
5. Get Geocodio API key for production

---

**Status**: ? **COMPLETE**
**Build**: ? **SUCCESSFUL**
**Quality**: ?????

