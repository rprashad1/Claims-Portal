namespace ClaimsPortal.Models;

public class Policy
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string RenewalNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string DoingBusinessAs { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Address Information
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    // Contact Information
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? CancelDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired, Cancelled
    public List<PolicyVehicle> Vehicles { get; set; } = [];
}

public class PolicyVehicle
{
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? PlateNumber { get; set; } = string.Empty;  // License plate number
    public string? PlateState { get; set; } = string.Empty;   // License plate state
    public bool IsActive { get; set; } = true;
}

public class ReportedBy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Insured, Brokers, Driver, Claimant, Other
}

public class ReportingMethod
{
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty; // Phone, Email, Mail, Fax, Agent Portal, Insured Portal
}
