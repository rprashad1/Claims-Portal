using ClaimsPortal.Data;
using ClaimsPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services;

/// <summary>
/// Database-connected implementation of Vendor Service for Vendor Master operations
/// Connects to VendorMaster and VendorAddress tables (separated from EntityMaster for performance)
/// </summary>
public class DatabaseVendorService : IVendorService
{
    private readonly ClaimsPortalDbContext _context;
    private readonly string _currentUser = "System"; // TODO: Replace with actual user from auth

    public DatabaseVendorService(ClaimsPortalDbContext context)
    {
        _context = context;
    }

    #region Search Operations

    public async Task<List<Vendor>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Vendor>();

        var vendors = await _context.VendorMasters
            .Include(v => v.Addresses)
            .Where(v => v.VendorStatus == 'Y' &&
                        (EF.Functions.Like(v.VendorName, $"%{searchTerm}%") ||
                         EF.Functions.Like(v.DoingBusinessAs ?? "", $"%{searchTerm}%")))
            .ToListAsync();

        return vendors.Select(MapToVendor).ToList();
    }

    public async Task<List<Vendor>> SearchByFeinAsync(string feinNumber)
    {
        if (string.IsNullOrWhiteSpace(feinNumber))
            return new List<Vendor>();

        var vendors = await _context.VendorMasters
            .Include(v => v.Addresses)
            .Where(v => v.VendorStatus == 'Y' &&
                        v.FEINNumber == feinNumber.Trim())
            .ToListAsync();

        return vendors.Select(MapToVendor).ToList();
    }

    public async Task<List<Vendor>> SearchVendorsAsync(string? searchTerm, VendorType? vendorType, VendorStatus? status)
    {
        var query = _context.VendorMasters
            .Include(v => v.Addresses)
            .AsQueryable();

        // Filter by status
        if (status.HasValue)
        {
            char statusChar = status.Value == VendorStatus.Active ? 'Y' : 'D';
            query = query.Where(v => v.VendorStatus == statusChar);
        }

        // Filter by vendor type
        if (vendorType.HasValue)
        {
            string vendorTypeStr = GetVendorTypeString(vendorType.Value);
            query = query.Where(v => v.VendorType == vendorTypeStr);
        }

        // Filter by search term (name, DBA, or FEIN)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(v =>
                EF.Functions.Like(v.VendorName, $"%{searchTerm}%") ||
                EF.Functions.Like(v.DoingBusinessAs ?? "", $"%{searchTerm}%") ||
                v.FEINNumber == searchTerm.Trim());
        }

        var vendors = await query.ToListAsync();
        return vendors.Select(MapToVendor).ToList();
    }

    #endregion

    #region CRUD Operations

    public async Task<Vendor?> GetVendorAsync(int vendorId)
    {
        var vendor = await _context.VendorMasters
            .Include(v => v.Addresses)
            .FirstOrDefaultAsync(v => v.VendorId == vendorId);

        return vendor != null ? MapToVendor(vendor) : null;
    }

    public async Task<List<Vendor>> GetAllVendorsAsync(VendorStatus? status = null)
    {
        var query = _context.VendorMasters
            .Include(v => v.Addresses)
            .AsQueryable();

        if (status.HasValue)
        {
            char statusChar = status.Value == VendorStatus.Active ? 'Y' : 'D';
            query = query.Where(v => v.VendorStatus == statusChar);
        }

        var vendors = await query.ToListAsync();
        return vendors.Select(MapToVendor).ToList();
    }

    public async Task<Vendor> CreateVendorAsync(Vendor vendor)
    {
        // Create the vendor entity first
        var vendorEntity = MapToVendorMasterEntity(vendor);
        vendorEntity.CreatedDate = DateTime.Now;
        vendorEntity.CreatedBy = _currentUser;

        _context.VendorMasters.Add(vendorEntity);
        await _context.SaveChangesAsync();

        // Now add the addresses with the new VendorId
        if (vendor.Addresses != null && vendor.Addresses.Any())
        {
            foreach (var address in vendor.Addresses)
            {
                var addrEntity = MapToVendorAddressEntity(address, vendorEntity.VendorId);
                addrEntity.CreatedDate = DateTime.Now;
                addrEntity.CreatedBy = _currentUser;
                _context.VendorAddresses.Add(addrEntity);
            }
            await _context.SaveChangesAsync();
        }

        // Reload with addresses to return complete vendor
        var createdVendor = await _context.VendorMasters
            .Include(v => v.Addresses)
            .FirstOrDefaultAsync(v => v.VendorId == vendorEntity.VendorId);

        return MapToVendor(createdVendor!);
    }

    public async Task<Vendor> UpdateVendorAsync(Vendor vendor)
    {
        var existingVendor = await _context.VendorMasters
            .Include(v => v.Addresses)
            .FirstOrDefaultAsync(v => v.VendorId == vendor.Id);

        if (existingVendor == null)
            throw new InvalidOperationException($"Vendor with ID {vendor.Id} not found");

        // Update vendor fields
        UpdateVendorEntityFromVendor(existingVendor, vendor);
        existingVendor.ModifiedDate = DateTime.Now;
        existingVendor.ModifiedBy = _currentUser;

        // Handle addresses - remove old, add new
        var existingAddresses = existingVendor.Addresses.ToList();
        foreach (var addr in existingAddresses)
        {
            _context.VendorAddresses.Remove(addr);
        }

        // Add new addresses
        if (vendor.Addresses != null && vendor.Addresses.Any())
        {
            foreach (var address in vendor.Addresses)
            {
                var addrEntity = MapToVendorAddressEntity(address, existingVendor.VendorId);
                addrEntity.CreatedDate = DateTime.Now;
                addrEntity.CreatedBy = _currentUser;
                _context.VendorAddresses.Add(addrEntity);
            }
        }

        await _context.SaveChangesAsync();

        // Reload to get updated data with addresses
        var updatedVendor = await _context.VendorMasters
            .Include(v => v.Addresses)
            .FirstOrDefaultAsync(v => v.VendorId == existingVendor.VendorId);

        return MapToVendor(updatedVendor!);
    }

    public async Task<bool> DisableVendorAsync(int vendorId)
    {
        var vendor = await _context.VendorMasters
            .FirstOrDefaultAsync(v => v.VendorId == vendorId);

        if (vendor == null)
            return false;

        vendor.VendorStatus = 'D';
        vendor.ModifiedDate = DateTime.Now;
        vendor.ModifiedBy = _currentUser;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableVendorAsync(int vendorId)
    {
        var vendor = await _context.VendorMasters
            .FirstOrDefaultAsync(v => v.VendorId == vendorId);

        if (vendor == null)
            return false;

        vendor.VendorStatus = 'Y';
        vendor.ModifiedDate = DateTime.Now;
        vendor.ModifiedBy = _currentUser;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Address Operations

    public async Task<VendorAddress> AddAddressAsync(int vendorId, VendorAddress address)
    {
        var vendor = await _context.VendorMasters
            .Include(v => v.Addresses)
            .FirstOrDefaultAsync(v => v.VendorId == vendorId);

        if (vendor == null)
            throw new InvalidOperationException($"Vendor with ID {vendorId} not found");

        // Check for existing main address
        if (address.AddressType == AddressTypeEnum.Main)
        {
            var existingMain = vendor.Addresses.Any(a => a.AddressType == 'M' && a.AddressStatus == 'Y');
            if (existingMain)
                throw new InvalidOperationException("Vendor can only have one main address");
        }

        var addrEntity = MapToVendorAddressEntity(address, vendorId);
        addrEntity.CreatedDate = DateTime.Now;
        addrEntity.CreatedBy = _currentUser;

        _context.VendorAddresses.Add(addrEntity);
        await _context.SaveChangesAsync();

        return MapToVendorAddress(addrEntity);
    }

    public async Task<VendorAddress> UpdateAddressAsync(int vendorId, VendorAddress address)
    {
        var addrEntity = await _context.VendorAddresses
            .FirstOrDefaultAsync(a => a.VendorAddressId == address.Id && a.VendorId == vendorId);

        if (addrEntity == null)
            throw new InvalidOperationException($"Address with ID {address.Id} not found for vendor {vendorId}");

        // Check for main address conflict
        if (address.AddressType == AddressTypeEnum.Main && GetAddressTypeChar(address.AddressType) != addrEntity.AddressType)
        {
            var existingMain = await _context.VendorAddresses
                .AnyAsync(a => a.VendorId == vendorId && a.AddressType == 'M' && a.VendorAddressId != address.Id && a.AddressStatus == 'Y');
            if (existingMain)
                throw new InvalidOperationException("Vendor can only have one main address");
        }

        UpdateAddressEntityFromVendorAddress(addrEntity, address);
        addrEntity.ModifiedDate = DateTime.Now;
        addrEntity.ModifiedBy = _currentUser;

        await _context.SaveChangesAsync();
        return MapToVendorAddress(addrEntity);
    }

    public async Task<bool> DeleteAddressAsync(int vendorId, int addressId)
    {
        var addrEntity = await _context.VendorAddresses
            .FirstOrDefaultAsync(a => a.VendorAddressId == addressId && a.VendorId == vendorId);

        if (addrEntity == null)
            return false;

        _context.VendorAddresses.Remove(addrEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<VendorAddress?> GetMainAddressAsync(int vendorId)
    {
        var addrEntity = await _context.VendorAddresses
            .FirstOrDefaultAsync(a => a.VendorId == vendorId && a.AddressType == 'M' && a.AddressStatus == 'Y');

        return addrEntity != null ? MapToVendorAddress(addrEntity) : null;
    }

    public async Task<List<VendorAddress>> GetActiveAddressesAsync(int vendorId)
    {
        var addresses = await _context.VendorAddresses
            .Where(a => a.VendorId == vendorId && a.AddressStatus == 'Y')
            .ToListAsync();

        return addresses.Select(MapToVendorAddress).ToList();
    }

    #endregion

    #region Validation Operations

    public async Task<bool> IsFeinUniqueAsync(string feinNumber, int? excludeVendorId = null)
    {
        var query = _context.VendorMasters
            .Where(v => v.FEINNumber == feinNumber.Trim());

        if (excludeVendorId.HasValue)
            query = query.Where(v => v.VendorId != excludeVendorId.Value);

        return !await query.AnyAsync();
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateVendorAsync(Vendor vendor)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vendor.Name))
            errors.Add("Vendor name is required");

        if (!vendor.EffectiveDate.HasValue)
            errors.Add("Effective date is required");

        if (!vendor.HasMainAddress)
            errors.Add("Vendor must have a main address");

        if (!string.IsNullOrWhiteSpace(vendor.FeinNumber))
        {
            bool isUnique = await IsFeinUniqueAsync(vendor.FeinNumber, vendor.Id > 0 ? vendor.Id : null);
            if (!isUnique)
                errors.Add("FEIN number already exists for another vendor");
        }

        return (errors.Count == 0, errors);
    }

    public async Task<bool> HasValidMainAddressAsync(int vendorId)
    {
        var mainAddress = await _context.VendorAddresses
            .FirstOrDefaultAsync(a => a.VendorId == vendorId && a.AddressType == 'M' && a.AddressStatus == 'Y');

        if (mainAddress == null)
            return false;

        return !string.IsNullOrWhiteSpace(mainAddress.StreetAddress) &&
               !string.IsNullOrWhiteSpace(mainAddress.City) &&
               !string.IsNullOrWhiteSpace(mainAddress.State) &&
               !string.IsNullOrWhiteSpace(mainAddress.ZipCode);
    }

    #endregion

    #region Mapping Methods

    private Vendor MapToVendor(VendorMasterEntity entity)
    {
        var vendor = new Vendor
        {
            Id = (int)entity.VendorId,
            Name = entity.VendorName ?? "",
            EntityType = entity.EntityType == 'B' ? EntityType.Business : EntityType.Individual,
            VendorType = ParseVendorType(entity.VendorType),
            DoingBusinessAs = entity.DoingBusinessAs,
            FeinNumber = entity.FEINNumber,
            EffectiveDate = entity.EffectiveDate,
            TerminationDate = entity.TerminationDate,
            Status = entity.VendorStatus == 'Y' ? VendorStatus.Active : VendorStatus.Disabled,
            CreatedDate = entity.CreatedDate,
            LastUpdatedDate = entity.ModifiedDate,
            Contact = new VendorContact
            {
                Name = entity.ContactName,
                BusinessPhone = entity.BusinessPhone,
                MobilePhone = entity.MobilePhone,
                FaxNumber = entity.FaxNumber,
                Email = entity.Email
            },
            TaxInfo = new TaxInformation
            {
                W9Received = entity.W9Received,
                SubjectTo1099 = entity.SubjectTo1099,
                BackupWithholding = entity.BackupWithholding
            },
            Payment = new VendorPayment
            {
                ReceivesBulkPayments = entity.ReceivesBulkPayment,
                Frequency = ParsePaymentFrequency(entity.PaymentFrequency)
            }
        };

        // Parse payment dates/days from PaymentDays field
        if (!string.IsNullOrEmpty(entity.PaymentDays))
        {
            vendor.Payment!.SelectedDates = ParsePaymentDates(entity.PaymentDays) ?? new List<int>();
            vendor.Payment!.SelectedDays = ParsePaymentDays(entity.PaymentDays) ?? new List<DayOfWeek>();
        }

        // Map addresses
        if (entity.Addresses != null)
        {
            vendor.Addresses = entity.Addresses
                .Where(a => a.AddressStatus == 'Y')
                .Select(MapToVendorAddress)
                .ToList();
        }

        return vendor;
    }

    private VendorMasterEntity MapToVendorMasterEntity(Vendor vendor)
    {
        var paymentDays = GetPaymentDaysString(vendor);

        return new VendorMasterEntity
        {
            VendorId = vendor.Id,
            EntityType = vendor.EntityType == EntityType.Business ? 'B' : 'I',
            VendorType = GetVendorTypeString(vendor.VendorType),
            VendorName = vendor.Name ?? string.Empty,
            DoingBusinessAs = vendor.DoingBusinessAs ?? string.Empty,
            FEINNumber = vendor.FeinNumber ?? string.Empty,
            EffectiveDate = vendor.EffectiveDate,
            TerminationDate = vendor.TerminationDate,
            ContactName = vendor.Contact?.Name ?? string.Empty,
            BusinessPhone = vendor.Contact?.BusinessPhone ?? string.Empty,
            MobilePhone = vendor.Contact?.MobilePhone ?? string.Empty,
            FaxNumber = vendor.Contact?.FaxNumber ?? string.Empty,
            Email = vendor.Contact?.Email ?? string.Empty,
            W9Received = vendor.TaxInfo?.W9Received ?? false,
            SubjectTo1099 = vendor.TaxInfo?.SubjectTo1099 ?? false,
            BackupWithholding = vendor.TaxInfo?.BackupWithholding ?? false,
            ReceivesBulkPayment = vendor.Payment?.ReceivesBulkPayments ?? false,
            PaymentFrequency = GetPaymentFrequencyString(vendor.Payment?.Frequency),
            PaymentDays = paymentDays ?? string.Empty,
            VendorStatus = vendor.Status == VendorStatus.Active ? 'Y' : 'D'
        };
    }

    private void UpdateVendorEntityFromVendor(VendorMasterEntity entity, Vendor vendor)
    {
        var paymentDays = GetPaymentDaysString(vendor);

        entity.EntityType = vendor.EntityType == EntityType.Business ? 'B' : 'I';
        entity.VendorType = GetVendorTypeString(vendor.VendorType);
        entity.VendorName = vendor.Name ?? string.Empty;
        entity.DoingBusinessAs = vendor.DoingBusinessAs ?? string.Empty;
        entity.FEINNumber = vendor.FeinNumber ?? string.Empty;
        entity.EffectiveDate = vendor.EffectiveDate;
        entity.TerminationDate = vendor.TerminationDate;
        entity.ContactName = vendor.Contact?.Name ?? string.Empty;
        entity.BusinessPhone = vendor.Contact?.BusinessPhone ?? string.Empty;
        entity.MobilePhone = vendor.Contact?.MobilePhone ?? string.Empty;
        entity.FaxNumber = vendor.Contact?.FaxNumber ?? string.Empty;
        entity.Email = vendor.Contact?.Email ?? string.Empty;
        entity.W9Received = vendor.TaxInfo?.W9Received ?? false;
        entity.SubjectTo1099 = vendor.TaxInfo?.SubjectTo1099 ?? false;
        entity.BackupWithholding = vendor.TaxInfo?.BackupWithholding ?? false;
        entity.ReceivesBulkPayment = vendor.Payment?.ReceivesBulkPayments ?? false;
        entity.PaymentFrequency = GetPaymentFrequencyString(vendor.Payment?.Frequency);
        entity.PaymentDays = paymentDays ?? string.Empty;
        entity.VendorStatus = vendor.Status == VendorStatus.Active ? 'Y' : 'D';
    }

    private VendorAddress MapToVendorAddress(VendorAddressEntity addr)
    {
        return new VendorAddress
        {
            Id = (int)addr.VendorAddressId,
            VendorId = (int)addr.VendorId,
            AddressType = GetAddressTypeEnum(addr.AddressType),
            Street1 = addr.StreetAddress,
            Street2 = addr.AddressLine2,
            City = addr.City,
            State = addr.State,
            ZipCode = addr.ZipCode,
            Country = addr.Country ?? "USA",
            Status = addr.AddressStatus == 'Y' ? AddressStatus.Active : AddressStatus.Disabled,
            CreatedDate = addr.CreatedDate,
            LastUpdatedDate = addr.ModifiedDate
        };
    }

    private VendorAddressEntity MapToVendorAddressEntity(VendorAddress address, long vendorId)
    {
        return new VendorAddressEntity
        {
            VendorId = vendorId,
            AddressType = GetAddressTypeChar(address.AddressType),
            StreetAddress = address.Street1,
            AddressLine2 = address.Street2,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country ?? "USA",
            AddressStatus = address.Status == AddressStatus.Active ? 'Y' : 'D'
        };
    }

    private void UpdateAddressEntityFromVendorAddress(VendorAddressEntity entity, VendorAddress address)
    {
        entity.AddressType = GetAddressTypeChar(address.AddressType);
        entity.StreetAddress = address.Street1;
        entity.AddressLine2 = address.Street2;
        entity.City = address.City;
        entity.State = address.State;
        entity.ZipCode = address.ZipCode;
        entity.Country = address.Country ?? "USA";
        entity.AddressStatus = address.Status == AddressStatus.Active ? 'Y' : 'D';
    }

    #endregion

    #region Helper Methods

    private static string GetVendorTypeString(VendorType vendorType)
    {
        return vendorType switch
        {
            VendorType.MedicalProvider => "Medical Provider",
            VendorType.Hospital => "Hospital",
            VendorType.DefenseAttorney => "Defense Attorney",
            VendorType.PlaintiffAttorney => "Plaintiff Attorney",
            VendorType.TowingService => "Towing Service",
            VendorType.RepairShop => "Repair Shop",
            VendorType.RentalCarCompany => "Rental Car Company",
            VendorType.InsuranceCarrier => "Insurance Carrier",
            VendorType.PoliceDepartment => "Police Department",
            VendorType.FireDepartment => "Fire Department",
            VendorType.Other => "Other",
            _ => "Other"
        };
    }

    private static VendorType ParseVendorType(string? vendorType)
    {
        return vendorType switch
        {
            "Medical Provider" or "Medical Providers" => VendorType.MedicalProvider,
            "Hospital" or "Hospitals" => VendorType.Hospital,
            "Defense Attorney" => VendorType.DefenseAttorney,
            "Plaintiff Attorney" or "Plaintiff Attorneys" => VendorType.PlaintiffAttorney,
            "Towing Service" or "Towing Services" => VendorType.TowingService,
            "Repair Shop" => VendorType.RepairShop,
            "Rental Car Company" => VendorType.RentalCarCompany,
            "Insurance Carrier" => VendorType.InsuranceCarrier,
            "Police Department" => VendorType.PoliceDepartment,
            "Fire Department" or "Fire Station" => VendorType.FireDepartment,
            _ => VendorType.Other
        };
    }

    private static string? GetPaymentFrequencyString(PaymentFrequency? frequency)
    {
        return frequency switch
        {
            PaymentFrequency.Monthly => "Monthly",
            PaymentFrequency.Weekly => "Weekly",
            _ => null
        };
    }

    private static PaymentFrequency? ParsePaymentFrequency(string? frequency)
    {
        return frequency?.ToLower() switch
        {
            "monthly" => PaymentFrequency.Monthly,
            "weekly" => PaymentFrequency.Weekly,
            _ => null
        };
    }

    private static char GetAddressTypeChar(AddressTypeEnum addressType)
    {
        return addressType switch
        {
            AddressTypeEnum.Main => 'M',
            AddressTypeEnum.Alternate => 'A',
            AddressTypeEnum.Temporary => 'T',
            _ => 'A'
        };
    }

    private static AddressTypeEnum GetAddressTypeEnum(char addressType)
    {
        return addressType switch
        {
            'M' => AddressTypeEnum.Main,
            'A' => AddressTypeEnum.Alternate,
            'T' => AddressTypeEnum.Temporary,
            _ => AddressTypeEnum.Alternate
        };
    }

    private string? GetPaymentDaysString(Vendor vendor)
    {
        if (vendor.Payment == null)
            return null;

        if (vendor.Payment.Frequency == PaymentFrequency.Monthly && vendor.Payment.SelectedDates.Any())
        {
            return string.Join(",", vendor.Payment.SelectedDates);
        }
        else if (vendor.Payment.Frequency == PaymentFrequency.Weekly && vendor.Payment.SelectedDays.Any())
        {
            return string.Join(",", vendor.Payment.SelectedDays);
        }

        return null;
    }

    private static List<int> ParsePaymentDates(string? datesString)
    {
        if (string.IsNullOrEmpty(datesString))
            return new List<int>();

        var dates = new List<int>();
        foreach (var part in datesString.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part.Trim(), out int day))
                dates.Add(day);
        }
        return dates;
    }

    private static List<DayOfWeek> ParsePaymentDays(string? daysString)
    {
        if (string.IsNullOrEmpty(daysString))
            return new List<DayOfWeek>();

        var days = new List<DayOfWeek>();
        foreach (var part in daysString.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<DayOfWeek>(part.Trim(), true, out var day))
                days.Add(day);
        }
        return days;
    }

    #endregion
}
