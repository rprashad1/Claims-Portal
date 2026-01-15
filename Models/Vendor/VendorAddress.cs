namespace ClaimsPortal.Models;

/// <summary>
/// Vendor Address model
/// A vendor can have multiple addresses (Main, Temporary, Alternate)
/// Only ONE main address is allowed per vendor
/// </summary>
public class VendorAddress
{
    /// <summary>
    /// Unique identifier for the address
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent Vendor
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Type of address (Main, Temporary, Alternate)
    /// Only one Main address allowed per vendor
    /// </summary>
    public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Main;

    /// <summary>
    /// Primary street address (Address 1)
    /// </summary>
    public string? Street1 { get; set; }

    /// <summary>
    /// Secondary address line (Address 2 - Apt, Suite, Unit, etc.)
    /// </summary>
    public string? Street2 { get; set; }

    /// <summary>
    /// City/Municipality name
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province code (2 characters for US states)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// ZIP/Postal code
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// Country code (defaults to USA)
    /// </summary>
    public string? Country { get; set; } = "USA";

    /// <summary>
    /// Status of this address (Active or Disabled)
    /// </summary>
    public AddressStatus Status { get; set; } = AddressStatus.Active;

    /// <summary>
    /// Timestamp when address was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when address was last updated
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }

    // Backward compatibility aliases
    public string? StreetAddress
    {
        get => Street1;
        set => Street1 = value;
    }

    public string? AddressLine2
    {
        get => Street2;
        set => Street2 = value;
    }

    /// <summary>
    /// Helper property to get/set AddressType as string for UI binding
    /// </summary>
    public string AddressTypeString
    {
        get => AddressType switch
        {
            AddressTypeEnum.Main => "Main",
            AddressTypeEnum.Temporary => "Temporary",
            AddressTypeEnum.Alternate => "Alternate",
            _ => "Main"
        };
        set => AddressType = value switch
        {
            "Main" => AddressTypeEnum.Main,
            "Temporary" => AddressTypeEnum.Temporary,
            "Alternate" => AddressTypeEnum.Alternate,
            _ => AddressTypeEnum.Main
        };
    }

    /// <summary>
    /// Helper property to get/set Status as string for UI binding
    /// </summary>
    public string StatusString
    {
        get => Status == AddressStatus.Active ? "Active" : "Disabled";
        set => Status = value == "Active" ? AddressStatus.Active : AddressStatus.Disabled;
    }

    /// <summary>
    /// Get formatted address string
    /// Returns: "Street, Suite, City, State ZIP"
    /// </summary>
    public string GetFormattedAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street1))
            parts.Add(Street1);

        if (!string.IsNullOrWhiteSpace(Street2))
            parts.Add(Street2);

        var cityStateZip = GetCityStateZip();
        if (!string.IsNullOrWhiteSpace(cityStateZip))
            parts.Add(cityStateZip);

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Get City, State ZIP formatted
    /// </summary>
    public string GetCityStateZip()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(City))
            parts.Add(City);

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        if (!string.IsNullOrWhiteSpace(ZipCode))
            return $"{string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)))} {ZipCode}".Trim();

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Check if address has minimum required information
    /// </summary>
    public bool HasAnyAddress => !string.IsNullOrWhiteSpace(Street1)
        || !string.IsNullOrWhiteSpace(City)
        || !string.IsNullOrWhiteSpace(State)
        || !string.IsNullOrWhiteSpace(ZipCode);

    /// <summary>
    /// Check if address is complete
    /// </summary>
    public bool IsComplete => !string.IsNullOrWhiteSpace(Street1)
        && !string.IsNullOrWhiteSpace(City)
        && !string.IsNullOrWhiteSpace(State)
        && !string.IsNullOrWhiteSpace(ZipCode);

    /// <summary>
    /// Create a copy of this address
    /// </summary>
    public VendorAddress Copy()
    {
        return new VendorAddress
        {
            Id = Id,
            VendorId = VendorId,
            AddressType = AddressType,
            Street1 = Street1,
            Street2 = Street2,
            City = City,
            State = State,
            ZipCode = ZipCode,
            Country = Country,
            Status = Status,
            CreatedDate = CreatedDate,
            LastUpdatedDate = LastUpdatedDate
        };
    }
}
