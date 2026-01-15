namespace ClaimsPortal.Models;

/// <summary>
/// Vendor Contact Information
/// Stores contact details for a vendor
/// </summary>
public class VendorContact
{
    /// <summary>
    /// Contact person/company name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Business phone number
    /// </summary>
    public string? BusinessPhone { get; set; }

    /// <summary>
    /// Fax number
    /// </summary>
    public string? FaxNumber { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    public string? MobilePhone { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Alias for Email (backward compatibility)
    /// </summary>
    public string? EmailAddress 
    { 
        get => Email; 
        set => Email = value; 
    }
}

/// <summary>
/// Tax Information for a vendor
/// Stores W9 and 1099 related information
/// </summary>
public class TaxInformation
{
    /// <summary>
    /// Whether IRS Form W-9 has been received
    /// </summary>
    public bool W9Received { get; set; }

    /// <summary>
    /// Whether vendor is subject to Form 1099-NEC reporting
    /// </summary>
    public bool SubjectTo1099 { get; set; }

    /// <summary>
    /// Whether backup withholding is required
    /// </summary>
    public bool BackupWithholding { get; set; }
}
