namespace ClaimsPortal.Models;

// ============================================================================
// UNIFIED INJURY MODEL - Core Building Block for All Parties
// ============================================================================

/// <summary>
/// Unified Injury model for all parties in the claims system.
/// ALL fields are OPTIONAL to support flexible data collection during initial claim reporting.
/// This is the single source of truth for injury information across the entire application.
/// 
/// Supports:
/// - Drivers (Insured and Third Party)
/// - Passengers (Insured and Third Party)
/// - Pedestrians
/// - Bicyclists
/// - Any other injured party
/// 
/// Design Philosophy:
/// - All fields optional (capture what you know at time of reporting)
/// - Reusable everywhere (no duplicated injury logic)
/// - Extensible (future party types automatically support injuries)
/// - Consistent validation (one place to update rules)
/// - Consistent UI (same injury form for all parties)
/// </summary>
public class Injury
{
    /// <summary>
    /// Type of injury (nature of injury) - from dropdown list
    /// Examples: "Head Injury", "Broken Arm", "Whiplash", "Concussion"
    /// Optional: May not be available during initial call
    /// </summary>
    public string? InjuryType { get; set; }

    /// <summary>
    /// Severity level of injury (1-5 scale)
    /// 1 = Minor, 2 = Mild, 3 = Moderate, 4 = Serious, 5 = Critical/Life-Threatening
    /// Optional: Defaults to 1 (Minor)
    /// </summary>
    public int SeverityLevel { get; set; } = 1;

    /// <summary>
    /// Detailed description of the injury
    /// Multi-line text for comprehensive injury details
    /// Optional: May be detailed later
    /// </summary>
    public string? InjuryDescription { get; set; }

    /// <summary>
    /// Indicates if the injury resulted in fatality
    /// Optional: Default false
    /// </summary>
    public bool IsFatality { get; set; } = false;

    /// <summary>
    /// Indicates if injured party was taken to hospital
    /// Optional: Default false
    /// </summary>
    public bool WasTakenToHospital { get; set; } = false;

    /// <summary>
    /// Hospital name where injured party was treated
    /// Optional: Only relevant if WasTakenToHospital = true
    /// </summary>
    public string? HospitalName { get; set; }

    /// <summary>
    /// Hospital street address
    /// Optional: Only relevant if WasTakenToHospital = true
    /// </summary>
    public string? HospitalStreetAddress { get; set; }

    /// <summary>
    /// Hospital city
    /// Optional: Only relevant if WasTakenToHospital = true
    /// </summary>
    public string? HospitalCity { get; set; }

    /// <summary>
    /// Hospital state
    /// Optional: Only relevant if WasTakenToHospital = true
    /// </summary>
    public string? HospitalState { get; set; }

    /// <summary>
    /// Hospital ZIP code
    /// Optional: Only relevant if WasTakenToHospital = true
    /// </summary>
    public string? HospitalZipCode { get; set; }

    /// <summary>
    /// Name of treating physician
    /// Optional: May not be available at initial reporting
    /// </summary>
    public string? TreatingPhysician { get; set; }

    /// <summary>
    /// Check if injury has minimum required information
    /// Returns true if injury type is populated
    /// </summary>
    public bool HasAnyInjury => !string.IsNullOrWhiteSpace(InjuryType);

    /// <summary>
    /// Check if injury has complete critical information
    /// Returns true if type and description provided
    /// </summary>
    public bool IsComplete => !string.IsNullOrWhiteSpace(InjuryType)
        && !string.IsNullOrWhiteSpace(InjuryDescription);

    /// <summary>
    /// Get formatted injury summary
    /// Returns: "Injury Type - Severity Level X - Description"
    /// </summary>
    public string GetFormattedSummary()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(InjuryType))
            parts.Add(InjuryType);

        parts.Add($"Severity {SeverityLevel}");

        if (!string.IsNullOrWhiteSpace(InjuryDescription))
            parts.Add(InjuryDescription);

        return string.Join(" - ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Get severity level description
    /// Returns human-readable severity description
    /// </summary>
    public string GetSeverityDescription()
    {
        return SeverityLevel switch
        {
            1 => "Minor",
            2 => "Mild",
            3 => "Moderate",
            4 => "Serious",
            5 => "Critical/Life-Threatening",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get hospital address formatted
    /// Returns: "Hospital Name, Street, City, State Zip"
    /// </summary>
    public string GetHospitalAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(HospitalName))
            parts.Add(HospitalName);

        if (!string.IsNullOrWhiteSpace(HospitalStreetAddress))
            parts.Add(HospitalStreetAddress);

        var cityStateZip = GetHospitalCityStateZip();
        if (!string.IsNullOrWhiteSpace(cityStateZip))
            parts.Add(cityStateZip);

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Get hospital city, state, zip formatted
    /// Returns: "City, State Zip"
    /// </summary>
    public string GetHospitalCityStateZip()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(HospitalCity))
            parts.Add(HospitalCity);

        if (!string.IsNullOrWhiteSpace(HospitalState))
            parts.Add(HospitalState);

        if (!string.IsNullOrWhiteSpace(HospitalZipCode))
            return $"{string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)))} {HospitalZipCode}".Trim();

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Create a copy of this injury
    /// Useful for initializing new party injury objects
    /// </summary>
    public Injury Copy()
    {
        return new Injury
        {
            InjuryType = InjuryType,
            SeverityLevel = SeverityLevel,
            InjuryDescription = InjuryDescription,
            IsFatality = IsFatality,
            WasTakenToHospital = WasTakenToHospital,
            HospitalName = HospitalName,
            HospitalStreetAddress = HospitalStreetAddress,
            HospitalCity = HospitalCity,
            HospitalState = HospitalState,
            HospitalZipCode = HospitalZipCode,
            TreatingPhysician = TreatingPhysician
        };
    }

    /// <summary>
    /// Get list of injury type options for dropdown
    /// </summary>
    public static List<string> GetInjuryTypes()
    {
        return new List<string>
        {
            "Abdominal/Internal",
            "Amputation",
            "Asphyxiation",
            "Back Injury",
            "Broken Arm",
            "Broken Leg",
            "Broken Rib",
            "Burns",
            "Chest Injury",
            "Concussion",
            "Contusion/Bruising",
            "Cut/Laceration",
            "Dental Injury",
            "Dislocation",
            "Eye Injury",
            "Fracture",
            "Head Injury",
            "Hearing Loss",
            "Hernia",
            "Internal Bleeding",
            "Joint Injury",
            "Ligament Injury",
            "Lump/Swelling",
            "Neck Injury",
            "Numbness",
            "Other",
            "Paralysis",
            "Puncture Wound",
            "Shoulder Injury",
            "Skin Damage",
            "Spinal Cord Injury",
            "Sprain/Strain",
            "Stab Wound",
            "Strangulation",
            "Stress Fracture",
            "Tendon Injury",
            "Thermal Burn",
            "Tingling",
            "Traumatic Brain Injury",
            "Whiplash",
            "Wound"
        };
    }

    /// <summary>
    /// Get list of severity level options for dropdown
    /// </summary>
    public static List<(int Value, string Label)> GetSeverityLevels()
    {
        return new List<(int Value, string Label)>
        {
            (1, "1 - Minor"),
            (2, "2 - Mild"),
            (3, "3 - Moderate"),
            (4, "4 - Serious"),
            (5, "5 - Critical/Life-Threatening")
        };
    }
}
