namespace ClaimsPortal.Services;

using ClaimsPortal.Models;

/// <summary>
/// Service for managing Vendor Master operations
/// Handles add, update, search, and disable operations
/// </summary>
public interface IVendorService
{
    // Search Operations
    /// <summary>
    /// Search vendors by name with smart/wildcard search support
    /// </summary>
    Task<List<Vendor>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Search vendors by FEIN number
    /// </summary>
    Task<List<Vendor>> SearchByFeinAsync(string feinNumber);

    /// <summary>
    /// Search vendors with combined criteria
    /// </summary>
    Task<List<Vendor>> SearchVendorsAsync(string? searchTerm, VendorType? vendorType, VendorStatus? status);

    // CRUD Operations
    /// <summary>
    /// Get vendor by ID
    /// </summary>
    Task<Vendor?> GetVendorAsync(int vendorId);

    /// <summary>
    /// Get all vendors (with optional status filter)
    /// </summary>
    Task<List<Vendor>> GetAllVendorsAsync(VendorStatus? status = null);

    /// <summary>
    /// Create a new vendor
    /// </summary>
    Task<Vendor> CreateVendorAsync(Vendor vendor);

    /// <summary>
    /// Update existing vendor
    /// </summary>
    Task<Vendor> UpdateVendorAsync(Vendor vendor);

    /// <summary>
    /// Disable a vendor (soft delete)
    /// Note: Vendors cannot be permanently deleted
    /// </summary>
    Task<bool> DisableVendorAsync(int vendorId);

    /// <summary>
    /// Enable a previously disabled vendor
    /// </summary>
    Task<bool> EnableVendorAsync(int vendorId);

    // Address Operations
    /// <summary>
    /// Add an address to a vendor
    /// Validates that only one main address exists
    /// </summary>
    Task<VendorAddress> AddAddressAsync(int vendorId, VendorAddress address);

    /// <summary>
    /// Update vendor address
    /// </summary>
    Task<VendorAddress> UpdateAddressAsync(int vendorId, VendorAddress address);

    /// <summary>
    /// Remove address from vendor
    /// </summary>
    Task<bool> DeleteAddressAsync(int vendorId, int addressId);

    /// <summary>
    /// Get vendor's main address
    /// </summary>
    Task<VendorAddress?> GetMainAddressAsync(int vendorId);

    /// <summary>
    /// Get all active addresses for vendor
    /// </summary>
    Task<List<VendorAddress>> GetActiveAddressesAsync(int vendorId);

    // Validation Operations
    /// <summary>
    /// Check if FEIN is unique (for new vendors or when updating)
    /// </summary>
    Task<bool> IsFeinUniqueAsync(string feinNumber, int? excludeVendorId = null);

    /// <summary>
    /// Validate vendor data completeness
    /// </summary>
    Task<(bool IsValid, List<string> Errors)> ValidateVendorAsync(Vendor vendor);

    /// <summary>
    /// Check if main address exists and is valid
    /// </summary>
    Task<bool> HasValidMainAddressAsync(int vendorId);
}

/// <summary>
/// Implementation of Vendor Service
/// Note: This is a mock implementation. In production, this would connect to a database.
/// </summary>
public class VendorService : IVendorService
{
    private readonly List<Vendor> _vendors = new();
    private int _nextVendorId = 1;
    private int _nextAddressId = 1;

    public async Task<List<Vendor>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await Task.FromResult(new List<Vendor>());

        var lowerSearch = searchTerm.ToLower().Trim();
        var results = _vendors.Where(v =>
            v.Status == VendorStatus.Active &&
            ((v.Name?.ToLower().Contains(lowerSearch) ?? false) ||
             (v.DoingBusinessAs?.ToLower().Contains(lowerSearch) ?? false))
        ).ToList();

        return await Task.FromResult(results);
    }

    public async Task<List<Vendor>> SearchByFeinAsync(string feinNumber)
    {
        if (string.IsNullOrWhiteSpace(feinNumber))
            return await Task.FromResult(new List<Vendor>());

        var results = _vendors.Where(v =>
            v.Status == VendorStatus.Active &&
            v.FeinNumber == feinNumber.Trim()
        ).ToList();

        return await Task.FromResult(results);
    }

    public async Task<List<Vendor>> SearchVendorsAsync(string? searchTerm, VendorType? vendorType, VendorStatus? status)
    {
        var query = _vendors.AsEnumerable();

        // Filter by status
        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);
        else
            query = query.Where(v => v.Status == VendorStatus.Active);

        // Filter by vendor type
        if (vendorType.HasValue)
            query = query.Where(v => v.VendorType == vendorType.Value);

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower().Trim();
            query = query.Where(v =>
                (v.Name?.ToLower().Contains(lowerSearch) ?? false) ||
                (v.DoingBusinessAs?.ToLower().Contains(lowerSearch) ?? false) ||
                v.FeinNumber == searchTerm.Trim()
            );
        }

        return await Task.FromResult(query.ToList());
    }

    public async Task<Vendor?> GetVendorAsync(int vendorId)
    {
        return await Task.FromResult(_vendors.FirstOrDefault(v => v.Id == vendorId));
    }

    public async Task<List<Vendor>> GetAllVendorsAsync(VendorStatus? status = null)
    {
        var query = _vendors.AsEnumerable();

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        return await Task.FromResult(query.ToList());
    }

    public async Task<Vendor> CreateVendorAsync(Vendor vendor)
    {
        vendor.Id = _nextVendorId++;
        vendor.CreatedDate = DateTime.UtcNow;
        vendor.LastUpdatedDate = DateTime.UtcNow;

        _vendors.Add(vendor);
        return await Task.FromResult(vendor);
    }

    public async Task<Vendor> UpdateVendorAsync(Vendor vendor)
    {
        var existing = _vendors.FirstOrDefault(v => v.Id == vendor.Id);
        if (existing == null)
            throw new InvalidOperationException($"Vendor with ID {vendor.Id} not found");

        vendor.LastUpdatedDate = DateTime.UtcNow;
        _vendors.Remove(existing);
        _vendors.Add(vendor);

        return await Task.FromResult(vendor);
    }

    public async Task<bool> DisableVendorAsync(int vendorId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult(false);

        vendor.Status = VendorStatus.Disabled;
        vendor.LastUpdatedDate = DateTime.UtcNow;

        return await Task.FromResult(true);
    }

    public async Task<bool> EnableVendorAsync(int vendorId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult(false);

        vendor.Status = VendorStatus.Active;
        vendor.LastUpdatedDate = DateTime.UtcNow;

        return await Task.FromResult(true);
    }

    public async Task<VendorAddress> AddAddressAsync(int vendorId, VendorAddress address)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            throw new InvalidOperationException($"Vendor with ID {vendorId} not found");

        // Check if trying to add multiple main addresses
        if (address.AddressType == AddressTypeEnum.Main)
        {
            var existingMain = vendor.Addresses.FirstOrDefault(a => a.AddressType == AddressTypeEnum.Main);
            if (existingMain != null)
                throw new InvalidOperationException("Vendor can only have one main address");
        }

        address.Id = _nextAddressId++;
        address.VendorId = vendorId;
        address.CreatedDate = DateTime.UtcNow;

        vendor.Addresses.Add(address);
        vendor.LastUpdatedDate = DateTime.UtcNow;

        return await Task.FromResult(address);
    }

    public async Task<VendorAddress> UpdateAddressAsync(int vendorId, VendorAddress address)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            throw new InvalidOperationException($"Vendor with ID {vendorId} not found");

        var existingAddress = vendor.Addresses.FirstOrDefault(a => a.Id == address.Id);
        if (existingAddress == null)
            throw new InvalidOperationException($"Address with ID {address.Id} not found");

        // Check if trying to change another address to main when main already exists
        if (address.AddressType == AddressTypeEnum.Main && existingAddress.AddressType != AddressTypeEnum.Main)
        {
            var existingMain = vendor.Addresses.FirstOrDefault(a => 
                a.AddressType == AddressTypeEnum.Main && a.Id != address.Id);
            if (existingMain != null)
                throw new InvalidOperationException("Vendor can only have one main address");
        }

        address.LastUpdatedDate = DateTime.UtcNow;
        vendor.Addresses.Remove(existingAddress);
        vendor.Addresses.Add(address);
        vendor.LastUpdatedDate = DateTime.UtcNow;

        return await Task.FromResult(address);
    }

    public async Task<bool> DeleteAddressAsync(int vendorId, int addressId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult(false);

        var address = vendor.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            return await Task.FromResult(false);

        vendor.Addresses.Remove(address);
        vendor.LastUpdatedDate = DateTime.UtcNow;

        return await Task.FromResult(true);
    }

    public async Task<VendorAddress?> GetMainAddressAsync(int vendorId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult<VendorAddress?>(null);

        var mainAddress = vendor.GetMainAddress();
        return await Task.FromResult(mainAddress);
    }

    public async Task<List<VendorAddress>> GetActiveAddressesAsync(int vendorId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult(new List<VendorAddress>());

        return await Task.FromResult(vendor.GetActiveAddresses());
    }

    public async Task<bool> IsFeinUniqueAsync(string feinNumber, int? excludeVendorId = null)
    {
        var isDuplicate = _vendors.Any(v =>
            v.FeinNumber == feinNumber.Trim() &&
            (!excludeVendorId.HasValue || v.Id != excludeVendorId.Value)
        );

        return await Task.FromResult(!isDuplicate);
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateVendorAsync(Vendor vendor)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vendor.Name))
            errors.Add("Vendor name is required");

        if (string.IsNullOrWhiteSpace(vendor.FeinNumber))
            errors.Add("FEIN number is required");

        if (!vendor.EffectiveDate.HasValue)
            errors.Add("Effective date is required");

        if (vendor.EntityType == EntityType.Business && string.IsNullOrWhiteSpace(vendor.DoingBusinessAs))
            errors.Add("Doing Business As is required for business entities");

        if (!vendor.HasMainAddress)
            errors.Add("Vendor must have a main address");

        if (vendor.Contact?.Name == null)
            errors.Add("Contact name is required");

        return await Task.FromResult((errors.Count == 0, errors));
    }

    public async Task<bool> HasValidMainAddressAsync(int vendorId)
    {
        var vendor = _vendors.FirstOrDefault(v => v.Id == vendorId);
        if (vendor == null)
            return await Task.FromResult(false);

        var mainAddress = vendor.GetMainAddress();
        if (mainAddress == null)
            return await Task.FromResult(false);

        return await Task.FromResult(mainAddress.IsComplete);
    }
}
