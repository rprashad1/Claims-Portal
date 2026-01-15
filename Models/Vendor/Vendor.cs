namespace ClaimsPortal.Models;

/// <summary>
/// Vendor Master model
/// Represents a business entity the insurance company does business with
/// Vendors are used across the application for payments, recoveries, and FNOL setup
/// A vendor can have multiple addresses but only ONE main address
/// </summary>
public class Vendor
{
    /// <summary>
    /// Unique identifier for the vendor
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of vendor (Medical Provider, Hospital, Attorney, etc.)
    /// </summary>
    public VendorType VendorType { get; set; }

    /// <summary>
    /// Entity type (Individual or Business)
    /// </summary>
    public EntityType EntityType { get; set; }

    /// <summary>
    /// Primary name of vendor
    /// For individuals: Person's name
    /// For businesses: Company name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Doing Business As (DBA)
    /// Only applicable for Business entities
    /// </summary>
    public string? DoingBusinessAs { get; set; }

    /// <summary>
    /// Federal Employer Identification Number
    /// Used for searching and identification
    /// </summary>
    public string? FeinNumber { get; set; }

    /// <summary>
    /// Date vendor relationship became effective
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Date vendor relationship ended
    /// Null if vendor is still active
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Current status of vendor (Active or Disabled)
    /// Note: Vendors cannot be deleted, only disabled
    /// </summary>
    public VendorStatus Status { get; set; } = VendorStatus.Active;

    /// <summary>
    /// Tax information (W9, 1099, withholding)
    /// </summary>
    public TaxInformation? TaxInfo { get; set; } = new();

    /// <summary>
    /// Contact information for the vendor
    /// </summary>
    public VendorContact? Contact { get; set; } = new();

    /// <summary>
    /// Payment information and preferences
    /// </summary>
    public VendorPayment? Payment { get; set; } = new();

    /// <summary>
    /// Collection of addresses for the vendor
    /// A vendor can have multiple addresses (Main, Temporary, Alternate)
    /// Only ONE main address is allowed
    /// </summary>
    public List<VendorAddress> Addresses { get; set; } = new();

    /// <summary>
    /// Timestamp when vendor was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when vendor was last updated
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }

    /// <summary>
    /// User ID of who created the vendor record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User ID of who last updated the vendor record
    /// </summary>
    public string? LastUpdatedBy { get; set; }

    // ============================================================================
    // Backward Compatibility Properties
    // ============================================================================

    /// <summary>
    /// Alias for TaxInfo.W9Received
    /// </summary>
    public bool W9Received
    {
        get => TaxInfo?.W9Received ?? false;
        set { if (TaxInfo != null) TaxInfo.W9Received = value; }
    }

    /// <summary>
    /// Alias for TaxInfo.SubjectTo1099
    /// </summary>
    public bool SubjectTo1099
    {
        get => TaxInfo?.SubjectTo1099 ?? false;
        set { if (TaxInfo != null) TaxInfo.SubjectTo1099 = value; }
    }

    /// <summary>
    /// Alias for TaxInfo.BackupWithholding
    /// </summary>
    public bool BackupWithholding
    {
        get => TaxInfo?.BackupWithholding ?? false;
        set { if (TaxInfo != null) TaxInfo.BackupWithholding = value; }
    }

    // ============================================================================
    // Helper Methods
    // ============================================================================

    /// <summary>
    /// Get the main address for this vendor
    /// </summary>
    public VendorAddress? GetMainAddress()
    {
        return Addresses.FirstOrDefault(a => a.AddressType == AddressTypeEnum.Main && a.Status == AddressStatus.Active);
    }

    /// <summary>
    /// Get all active addresses
    /// </summary>
    public List<VendorAddress> GetActiveAddresses()
    {
        return Addresses.Where(a => a.Status == AddressStatus.Active).ToList();
    }

    /// <summary>
    /// Check if vendor has a main address
    /// </summary>
    public bool HasMainAddress => GetMainAddress() != null;

    /// <summary>
    /// Get display name (considers DBA for businesses)
    /// </summary>
    public string GetDisplayName()
    {
        if (EntityType == EntityType.Business && !string.IsNullOrWhiteSpace(DoingBusinessAs))
            return $"{Name} ({DoingBusinessAs})";
        return Name ?? "Unknown Vendor";
    }

    /// <summary>
    /// Get primary contact phone (prefers business phone, then mobile)
    /// </summary>
    public string? GetPrimaryPhone()
    {
        return !string.IsNullOrWhiteSpace(Contact?.BusinessPhone) 
            ? Contact.BusinessPhone 
            : Contact?.MobilePhone;
    }

    /// <summary>
    /// Check if vendor is searchable (has name or FEIN)
    /// </summary>
    public bool IsSearchable => !string.IsNullOrWhiteSpace(Name) || !string.IsNullOrWhiteSpace(FeinNumber);

    /// <summary>
    /// Check if vendor record is complete
    /// </summary>
    public bool IsComplete => 
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(FeinNumber) &&
        EffectiveDate.HasValue &&
        HasMainAddress &&
        Contact?.Name != null;

    /// <summary>
    /// Create a deep copy of this vendor
    /// </summary>
    public Vendor Copy()
    {
        return new Vendor
        {
            Id = Id,
            VendorType = VendorType,
            EntityType = EntityType,
            Name = Name,
            DoingBusinessAs = DoingBusinessAs,
            FeinNumber = FeinNumber,
            EffectiveDate = EffectiveDate,
            TerminationDate = TerminationDate,
            Status = Status,
            TaxInfo = new TaxInformation
            {
                W9Received = TaxInfo?.W9Received ?? false,
                SubjectTo1099 = TaxInfo?.SubjectTo1099 ?? false,
                BackupWithholding = TaxInfo?.BackupWithholding ?? false
            },
            Contact = new VendorContact
            {
                Name = Contact?.Name,
                BusinessPhone = Contact?.BusinessPhone,
                FaxNumber = Contact?.FaxNumber,
                MobilePhone = Contact?.MobilePhone,
                Email = Contact?.Email
            },
            Payment = new VendorPayment
            {
                ReceivesBulkPayments = Payment?.ReceivesBulkPayments ?? false,
                Frequency = Payment?.Frequency,
                SelectedDates = new List<int>(Payment?.SelectedDates ?? new List<int>()),
                SelectedDays = new List<DayOfWeek>(Payment?.SelectedDays ?? new List<DayOfWeek>())
            },
            Addresses = Addresses.Select(a => a.Copy()).ToList(),
            CreatedDate = CreatedDate,
            LastUpdatedDate = LastUpdatedDate,
            CreatedBy = CreatedBy,
            LastUpdatedBy = LastUpdatedBy
        };
    }
}
