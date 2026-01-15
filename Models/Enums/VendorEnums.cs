namespace ClaimsPortal.Models;

/// <summary>
/// Vendor Type enumeration for classifying business partners
/// </summary>
public enum VendorType
{
    MedicalProvider,
    Hospital,
    DefenseAttorney,
    PlaintiffAttorney,
    TowingService,
    RepairShop,
    RentalCarCompany,
    InsuranceCarrier,
    PoliceDepartment,
    FireDepartment,
    Other
}

/// <summary>
/// Entity Type enumeration for individual vs business classification
/// </summary>
public enum EntityType
{
    Individual,
    Business
}

/// <summary>
/// Address Type enumeration for different address purposes
/// </summary>
public enum AddressTypeEnum
{
    Main,
    Temporary,
    Alternate
}

/// <summary>
/// Address Status enumeration
/// </summary>
public enum AddressStatus
{
    Active,
    Disabled
}

/// <summary>
/// Vendor Status enumeration
/// </summary>
public enum VendorStatus
{
    Active,
    Disabled
}

/// <summary>
/// Payment Frequency enumeration (renamed from BulkPaymentFrequency for consistency)
/// </summary>
public enum PaymentFrequency
{
    Monthly,
    Weekly
}

/// <summary>
/// Bulk Payment Frequency enumeration (alias for backward compatibility)
/// </summary>
public enum BulkPaymentFrequency
{
    Monthly,
    Weekly
}

/// <summary>
/// Day of week enumeration for weekly payments
/// </summary>
public enum PaymentDay
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday
}
