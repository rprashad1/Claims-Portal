namespace ClaimsPortal.Models;

// ============================================================================
// BASE TEMPLATES - Reusable across all parties
// ============================================================================

/// <summary>
/// Unified party/person model for all parties in a claim
/// Supports both individuals and businesses
/// </summary>
public class Party
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty; // Reported By, Witness, Insured Driver, Passenger, etc.
    public string PartyRole { get; set; } = string.Empty; // Vehicle Owner, Pedestrian, Bicyclist, Property Owner, etc.

    // Individual/Business Type
    public string EntityType { get; set; } = "Individual"; // Individual or Business
    public string? BusinessName { get; set; } = string.Empty; // Business name if EntityType is Business
    public string? DoingBusinessAs { get; set; } = string.Empty; // DBA if applicable

    // Contact Information
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; } = string.Empty; // Secondary address (optional)
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public string? Phone2 { get; set; } = string.Empty; // Secondary phone (optional)
    public string Email { get; set; } = string.Empty;

    // Identification
    public string FeinSsNumber { get; set; } = string.Empty; // FEIN for business, SS# for individual
    public string? LicenseNumber { get; set; } = string.Empty; // Driver license, if applicable
    public string? LicenseState { get; set; } = string.Empty; // License state, if applicable

    // Personal Information
    public DateTime? DateOfBirth { get; set; } // Optional, for individuals

    // Metadata
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Unified injury information template for all parties
/// Reusable across drivers, passengers, pedestrians, bicyclists, etc.
/// </summary>
public class InjuryRecord
{
    public int Id { get; set; }
    public string InjuryType { get; set; } = string.Empty; // Nature of injury
    public int SeverityLevel { get; set; } = 1; // Severity: 1 (Minor), 2, 3, 4, 5 (Critical)
    public DateTime DateOfInjury { get; set; } = DateTime.Now;
    public string InjuryDescription { get; set; } = string.Empty; // Multi-line description

    // Medical Information
    public DateTime FirstMedicalTreatmentDate { get; set; }
    public bool IsFatality { get; set; }
    public bool WasTakenToHospital { get; set; }
    public string? HospitalName { get; set; } = string.Empty;
    public string? HospitalAddress { get; set; } = string.Empty;
    public string? HospitalCity { get; set; } = string.Empty;
    public string? HospitalState { get; set; } = string.Empty;
    public string? HospitalZipCode { get; set; } = string.Empty;

    // Additional Information
    public string? TreatingPhysician { get; set; } = string.Empty;
    public string? PreexistingConditions { get; set; } = string.Empty; // Multi-line
}

/// <summary>
/// Attorney/Representative information template
/// Reusable across all parties who have legal representation
/// </summary>
public class Attorney
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirmName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; } = string.Empty;
    public string? LicenseState { get; set; } = string.Empty;
}

/// <summary>
/// Address Search Result from Geocoding Service
/// </summary>
public class AddressSearchResult
{
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string County { get; set; } = string.Empty;
    public string Accuracy { get; set; } = string.Empty; // Accuracy level from geocoding service
}

// ============================================================================
// LEGACY MODELS - Keep for backward compatibility
// ============================================================================

public class Witness
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FeinSsNumber { get; set; } = string.Empty;
}

public class Authority
{
    public int Id { get; set; }
    public string AuthorityName { get; set; } = string.Empty; // Police Department, Fire Department
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string? ReportNumber { get; set; } = string.Empty; // Case/Report number from authority
    // If the authority was selected from Vendor Master, this holds the VendorId
    public long? VendorId { get; set; }
}

public class ClaimLossDetails
{
    public DateTime DateOfLoss { get; set; }
    public TimeOnly TimeOfLoss { get; set; }
    public DateTime ReportDate { get; set; }
    public TimeOnly ReportTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Location2 { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string ReportingMethod { get; set; } = string.Empty;

    // If Reported By is "Other" - use unified Address class (ALL OPTIONAL)
    public string? ReportedByName { get; set; }
    public Address? ReportedByAddress { get; set; } = new(); // ? Now using unified Address class
    public string? ReportedByPhone { get; set; }
    public string? ReportedByEmail { get; set; }
    public string? ReportedByFeinSsNumber { get; set; }

    public List<Witness> Witnesses { get; set; } = [];
    public List<Authority> AuthoritiesNotified { get; set; } = [];
    public bool HasOtherVehiclesInvolved { get; set; }
    public bool HasInjuries { get; set; }
    public bool HasPropertyDamage { get; set; }

    public string CauseOfLoss { get; set; } = string.Empty;
    public string LossDescription { get; set; } = string.Empty;
    public string WeatherCondition { get; set; } = string.Empty;
}

public class InsuredPartyInfo
{
    public string Name { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Individual, Business
    public string DoingBusinessAs { get; set; } = string.Empty; // Only if Business
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string FeinSsNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class VehicleInfo
{
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? PlateNumber { get; set; } = string.Empty; // License Plate Number
    public string? PlateState { get; set; } = string.Empty;  // License Plate State
    public bool IsDamaged { get; set; }
    public bool IsDrivable { get; set; }
    public bool IsListed { get; set; } = true;

    // Entity references for vehicle owner and driver
    public long? VehicleOwnerEntityId { get; set; }  // FK to EntityMaster - Vehicle Owner
    public long? DriverEntityId { get; set; }        // FK to EntityMaster - Driver (if different from owner)

    // Vehicle condition fields
    public bool WasTowed { get; set; }
    public bool InStorage { get; set; }
    public string? StorageLocation { get; set; }
    public bool HasDashCam { get; set; }

    // Vehicle damage fields
    public string DamageDetails { get; set; } = string.Empty;
    public bool DidVehicleRollOver { get; set; }
    public bool HadWaterDamage { get; set; }
    public bool AreHeadlightsOn { get; set; }
    public bool DidAirbagDeploy { get; set; }

    // Compatibility properties for older code expecting different names
    public bool HeadlightsWereOn { get => AreHeadlightsOn; set => AreHeadlightsOn = value; }
    public bool AirbagDeployed { get => DidAirbagDeploy; set => DidAirbagDeploy = value; }
}

public class DriverInfo
{
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public bool IsListed { get; set; } = true;
    public DateTime DateOfBirth { get; set; }
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FeinSsNumber { get; set; } = string.Empty;
}

public class InjuryInfo
{
    public string NatureOfInjury { get; set; } = string.Empty;
    public DateTime FirstMedicalTreatmentDate { get; set; }
    public bool IsFatality { get; set; }
    public bool WasTakenToHospital { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalAddress { get; set; } = string.Empty;
    public string InjuryDescription { get; set; } = string.Empty;
    public int SeverityLevel { get; set; } = 1;
}

public class AttorneyInfo
{
    /// <summary>
    /// If attorney was selected from Vendor Master, this is the EntityId.
    /// If null, attorney was manually entered and needs to be created.
    /// </summary>
    public long? VendorEntityId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string FirmName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
}

public class SubClaim
{
    public int Id { get; set; }
    public int FeatureNumber { get; set; } // 1, 2, 3, etc.
    public string Coverage { get; set; } = string.Empty; // BI, PD, PIP, APIP, UM
    public string CoverageLimits { get; set; } = string.Empty; // 100/300, 50,000, etc.
    public string ClaimantName { get; set; } = string.Empty;
    public decimal ExpenseReserve { get; set; }
    public decimal IndemnityReserve { get; set; }
    public string AssignedAdjusterId { get; set; } = string.Empty;
    public string AssignedAdjusterName { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; }
    public string ClaimType { get; set; } = string.Empty; // Driver, Passenger, ThirdParty

    // Financial fields for grid display
    public decimal Deductible { get; set; }
    public decimal Offset { get; set; }
    public decimal IndemnityPaid { get; set; }
    public decimal ExpensePaid { get; set; }
    public decimal SalvageReserve { get; set; }
    public decimal SubrogationReserve { get; set; }
    public decimal Recoveries { get; set; }
}

public class Claim
{
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, CIQ (Claim Inquiry), Investigation, etc.
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public ClaimLossDetails LossDetails { get; set; } = new();
    public Policy PolicyInfo { get; set; } = new();
    public InsuredPartyInfo InsuredParty { get; set; } = new();
    public VehicleInfo InsuredVehicle { get; set; } = new();
    public DriverInfo Driver { get; set; } = new();
    public Injury? DriverInjury { get; set; } // ? NOW using unified Injury class
    public AttorneyInfo? DriverAttorney { get; set; }
    public List<InsuredPassenger> Passengers { get; set; } = [];
    public List<ThirdParty> ThirdParties { get; set; } = [];
    public List<PropertyDamage> PropertyDamages { get; set; } = [];
    public List<SubClaim> SubClaims { get; set; } = [];

    // New party-based collections (future use)
    public List<Party> Parties { get; set; } = [];
    public List<InjuryRecord> InjuryRecords { get; set; } = [];
}

public class InsuredPassenger
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FeinSsNumber { get; set; } = string.Empty;
    public bool WasInjured { get; set; }
    public Injury? InjuryInfo { get; set; } // ? NOW using unified Injury class (ALL OPTIONAL)
    public bool HasAttorney { get; set; }
    public AttorneyInfo? AttorneyInfo { get; set; }
    /// <summary>
    /// Selected VIN when this party is a passenger (references one of the third-party vehicles)
    /// </summary>
    public string? SelectedVehicleVin { get; set; } = string.Empty;
}

public class ThirdParty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Vehicle, Pedestrian, Bicyclist, Property, Other
    public Address Address { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? FeinSsNumber { get; set; } = string.Empty;

    // Third Party Vehicle Insurance Information (optional - for Vehicle type)
    public string? InsuranceCarrier { get; set; } = string.Empty;
    public string? InsurancePolicyNumber { get; set; } = string.Empty;

    public VehicleInfo? Vehicle { get; set; }
    public DriverInfo? Driver { get; set; }
    /// <summary>
    /// Selected VIN when this third party record is a passenger (references one of the third-party vehicles)
    /// </summary>
    public string? SelectedVehicleVin { get; set; } = string.Empty;

    /// <summary>
    /// For Vehicle type: Identifies who is the injured party (and therefore the claimant).
    /// Values: "Owner" or "Driver"
    /// The injured party is always the claimant.
    /// </summary>
    public string? InjuredParty { get; set; } = string.Empty; // "Owner" or "Driver"

    public bool WasInjured { get; set; }
    public Injury? InjuryInfo { get; set; } // ? NOW using unified Injury class (ALL OPTIONAL)
    public bool HasAttorney { get; set; }
    public AttorneyInfo? AttorneyInfo { get; set; }

    /// <summary>
    /// Gets the claimant name based on who is the injured party.
    /// For Vehicle type: Returns the name of the injured party (Owner or Driver).
    /// For non-Vehicle types: Returns the Name (the third party person is the claimant).
    /// </summary>
    public string GetClaimantName()
    {
        if (Type != "Vehicle" || !WasInjured)
            return Name;

        if (InjuredParty == "Driver" && Driver != null && !string.IsNullOrEmpty(Driver.Name))
            return Driver.Name;

        return Name; // Default to Owner
    }

    /// <summary>
    /// Determines if the driver is the same person as the owner.
    /// </summary>
    public bool IsDriverSameAsOwner => Driver == null || string.IsNullOrEmpty(Driver.Name) || Driver.Name == Name;
}

public class Adjuster
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Territory { get; set; } = string.Empty;
    public int CurrentWorkload { get; set; }
}

public class CoverageOption
{
    public string Code { get; set; } = string.Empty; // BI, PD, PIP, APIP, UM
    public string DisplayName { get; set; } = string.Empty;
    public string Limits { get; set; } = string.Empty; // 100/300, 50,000, etc.
}

public class PropertyDamage
{
    public int Id { get; set; }
    public string PropertyType { get; set; } = string.Empty; // Building, Fence, Other
    public string Description { get; set; } = string.Empty; // Property Description

    // Property Owner Information
    public string OwnerName { get; set; } = string.Empty;
    public Address OwnerAddress { get; set; } = new(); // ? Now using unified Address class (ALL OPTIONAL)
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerFeinSsNumber { get; set; } = string.Empty; // Optional

    // Property Location (can be different from owner address)
    public string PropertyLocation { get; set; } = string.Empty;
    public Address PropertyAddress { get; set; } = new(); // Can capture separate property address (ALL OPTIONAL)

    public string DamageDescription { get; set; } = string.Empty;
    public decimal EstimatedDamage { get; set; } // Optional - not required for Save & Create Feature
}
