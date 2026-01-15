namespace ClaimsPortal.Models;

/// <summary>
/// Hospital information returned from vendor search
/// </summary>
public class HospitalInfo
{
    /// <summary>
    /// EntityId from EntityMaster if selected from vendor search
    /// Null if manually entered
    /// </summary>
    public long? EntityId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Get formatted address
    /// </summary>
    public string GetFormattedAddress()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(StreetAddress)) parts.Add(StreetAddress);
        
        var cityStateZip = new List<string>();
        if (!string.IsNullOrEmpty(City)) cityStateZip.Add(City);
        if (!string.IsNullOrEmpty(State)) cityStateZip.Add(State);
        if (!string.IsNullOrEmpty(ZipCode)) cityStateZip.Add(ZipCode);
        
        if (cityStateZip.Count > 0)
            parts.Add(string.Join(", ", cityStateZip));
        
        return string.Join(", ", parts);
    }
}
