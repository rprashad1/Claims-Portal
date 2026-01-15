namespace ClaimsPortal.Models;

// ============================================================================
// UNIFIED ADDRESS MODEL - Core Building Block for All Entities
// ============================================================================

/// <summary>
/// Unified Address model for all parties and entities in the claims system.
/// ALL fields are OPTIONAL to support flexible data collection during initial claim reporting.
/// This is the single source of truth for address information across the entire application.
/// 
/// Supports:
/// - Individual persons (Reported By, Witnesses, Drivers, Passengers, etc.)
/// - Business entities (Property owners, businesses, etc.)
/// - Professional addresses (Attorneys, adjusters, medical facilities, etc.)
/// 
/// Design Philosophy:
/// - All fields optional (capture what you know at time of reporting)
/// - Reusable everywhere (no duplicated address logic)
/// - Extensible (future entities automatically support addresses)
/// - Consistent validation (one place to update rules)
/// </summary>
public class Address
{
    /// <summary>
    /// Primary street address (Street number and name)
    /// Optional: May not be available during initial call
    /// </summary>
    public string? StreetAddress { get; set; }

    /// <summary>
    /// Secondary address line (Apartment, Suite, Unit, etc.)
    /// Optional: Only used when applicable
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City/Municipality name
    /// Optional: May not be available during initial call
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province code (typically 2 characters for US states)
    /// Optional: May not be available during initial call
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// ZIP/Postal code
    /// Optional: May not be available during initial call
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// Country (defaults to USA, can be extended for international)
    /// Optional: Defaults to "USA" for current implementation
    /// </summary>
    public string? Country { get; set; } = "USA";

    /// <summary>
    /// County name (useful for jurisdiction determination)
    /// Optional: Automatically populated from address lookup service
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// Latitude coordinate from geocoding service
    /// Optional: Only set when address lookup is performed
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate from geocoding service
    /// Optional: Only set when address lookup is performed
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Address accuracy level from geocoding service
    /// Examples: "rooftop", "range_interpolated", "geometric_center", "approximate"
    /// Optional: Only set when address lookup is performed
    /// </summary>
    public string? AddressAccuracy { get; set; }

    /// <summary>
    /// Indicates if address has been verified/validated
    /// Useful for follow-up or additional validation needed
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Timestamp when address was last updated
    /// Useful for tracking information freshness
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }

    /// <summary>
    /// Check if address has minimum required information
    /// Returns true if at least one field is populated
    /// </summary>
    public bool HasAnyAddress => !string.IsNullOrWhiteSpace(StreetAddress)
        || !string.IsNullOrWhiteSpace(City)
        || !string.IsNullOrWhiteSpace(State)
        || !string.IsNullOrWhiteSpace(ZipCode);

    /// <summary>
    /// Check if address has complete information
    /// Returns true if all primary fields are populated (Street, City, State, Zip)
    /// </summary>
    public bool IsComplete => !string.IsNullOrWhiteSpace(StreetAddress)
        && !string.IsNullOrWhiteSpace(City)
        && !string.IsNullOrWhiteSpace(State)
        && !string.IsNullOrWhiteSpace(ZipCode);

    /// <summary>
    /// Get formatted address string
    /// Returns formatted: "Street Address, Address2, City, State ZipCode"
    /// </summary>
    public string GetFormattedAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(StreetAddress))
            parts.Add(StreetAddress);

        if (!string.IsNullOrWhiteSpace(AddressLine2))
            parts.Add(AddressLine2);

        var cityStateZip = GetCityStateZip();
        if (!string.IsNullOrWhiteSpace(cityStateZip))
            parts.Add(cityStateZip);

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Get City, State ZipCode formatted
    /// Returns: "City, State ZipCode"
    /// </summary>
    public string GetCityStateZip()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(City))
            parts.Add(City);

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        var cityState = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        
        if (!string.IsNullOrWhiteSpace(ZipCode))
        {
            if (!string.IsNullOrWhiteSpace(cityState))
                return $"{cityState} {ZipCode}";
            return ZipCode;
        }

        return cityState;
    }

    /// <summary>
    /// Create a copy of this address
    /// Useful for initializing new party objects
    /// </summary>
    public Address Copy()
    {
        return new Address
        {
            StreetAddress = StreetAddress,
            AddressLine2 = AddressLine2,
            City = City,
            State = State,
            ZipCode = ZipCode,
            Country = Country,
            County = County,
            Latitude = Latitude,
            Longitude = Longitude,
            AddressAccuracy = AddressAccuracy,
            IsVerified = IsVerified,
            LastUpdatedDate = LastUpdatedDate
        };
    }
}
