using ClaimsPortal.Data;
using ClaimsPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    /// <summary>
    /// Service for searching vendors in the VendorMaster table.
    /// Provides unified search across all vendor types (Attorneys, Hospitals, Authorities, etc.)
    /// with duplicate detection to prevent duplicate entries.
    /// </summary>
    public interface IVendorSearchService
    {
        /// <summary>
        /// Search vendors by name and type
        /// </summary>
        Task<List<VendorSearchResult>> SearchVendorsAsync(string searchTerm, string? vendorType = null, int maxResults = 20);
        
        /// <summary>
        /// Search hospitals specifically
        /// </summary>
        Task<List<VendorSearchResult>> SearchHospitalsAsync(string searchTerm, int maxResults = 20);
        
        /// <summary>
        /// Search attorneys (both defense and plaintiff)
        /// </summary>
        Task<List<VendorSearchResult>> SearchAttorneysAsync(string searchTerm, string? attorneyType = null, int maxResults = 20);
        
        /// <summary>
        /// Search authorities (police, fire departments)
        /// </summary>
        Task<List<VendorSearchResult>> SearchAuthoritiesAsync(string searchTerm, string? authorityType = null, int maxResults = 20);
        
        /// <summary>
        /// Check if a vendor already exists by name (case-insensitive)
        /// Returns matching vendors if found
        /// </summary>
        Task<List<VendorSearchResult>> CheckDuplicateAsync(string name, string? vendorType = null);
        
        /// <summary>
        /// Get vendor by VendorId
        /// </summary>
        Task<VendorSearchResult?> GetVendorByIdAsync(long vendorId);
    }
    
    /// <summary>
    /// Unified search result model for all vendor types
    /// </summary>
    public class VendorSearchResult
    {
        public long VendorId { get; set; }
        public long EntityId { get; set; }  // For backward compatibility
        public string Name { get; set; } = string.Empty;
        public string? DoingBusinessAs { get; set; }
        public string VendorType { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FeinNumber { get; set; }
        
        // Address
        public string? StreetAddress { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        
        // Contact
        public string? ContactName { get; set; }
        
        // Status
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Get formatted address string
        /// </summary>
        public string GetFormattedAddress()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(StreetAddress)) parts.Add(StreetAddress);
            if (!string.IsNullOrEmpty(AddressLine2)) parts.Add(AddressLine2);
            
            var cityStateZip = new List<string>();
            if (!string.IsNullOrEmpty(City)) cityStateZip.Add(City);
            if (!string.IsNullOrEmpty(State)) cityStateZip.Add(State);
            if (!string.IsNullOrEmpty(ZipCode)) cityStateZip.Add(ZipCode);
            
            if (cityStateZip.Count > 0)
                parts.Add(string.Join(", ", cityStateZip));
            
            return string.Join(", ", parts);
        }
        
        /// <summary>
        /// Get short address (City, State)
        /// </summary>
        public string GetShortAddress()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(State)) parts.Add(State);
            if (!string.IsNullOrEmpty(ZipCode)) parts.Add(ZipCode);
            return string.Join(", ", parts);
        }
    }

    public class VendorSearchService : IVendorSearchService
    {
        private readonly ClaimsPortalDbContext _context;
        
        // Vendor type constants
        public static class VendorTypes
        {
            public const string Hospital = "Hospital";
            public const string MedicalProvider = "Medical Provider";
            public const string DefenseAttorney = "Defense Attorney";
            public const string PlaintiffAttorney = "Plaintiff Attorney";
            public const string PoliceDepartment = "Police Department";
            public const string FireDepartment = "Fire Department";
            public const string TowingService = "Towing Service";
            public const string RepairShop = "Repair Shop";
            public const string RentalCarCompany = "Rental Car Company";
            public const string InsuranceCarrier = "Insurance Carrier";
            public const string Other = "Other";
        }

        public VendorSearchService(ClaimsPortalDbContext context)
        {
            _context = context;
        }

        public async Task<List<VendorSearchResult>> SearchVendorsAsync(string searchTerm, string? vendorType = null, int maxResults = 20)
        {
            var query = _context.VendorMasters
                .Include(v => v.Addresses)
                .Where(v => v.VendorStatus == 'Y');
            
            // Filter by vendor type if specified
            if (!string.IsNullOrEmpty(vendorType))
            {
                query = query.Where(v => v.VendorType == vendorType);
            }
            
            // Search by name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(v => 
                    (v.VendorName != null && v.VendorName.ToLower().Contains(searchLower)) ||
                    (v.DoingBusinessAs != null && v.DoingBusinessAs.ToLower().Contains(searchLower)) ||
                    (v.FEINNumber != null && v.FEINNumber.Contains(searchTerm)));
            }
            
            var vendors = await query
                .OrderBy(v => v.VendorName)
                .Take(maxResults)
                .ToListAsync();
            
            return vendors.Select(MapToSearchResult).ToList();
        }

        public async Task<List<VendorSearchResult>> SearchHospitalsAsync(string searchTerm, int maxResults = 20)
        {
            var query = _context.VendorMasters
                .Include(v => v.Addresses)
                .Where(v => v.VendorStatus == 'Y' &&
                    (v.VendorType == VendorTypes.Hospital || 
                     v.VendorType == VendorTypes.MedicalProvider));
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(v => 
                    (v.VendorName != null && v.VendorName.ToLower().Contains(searchLower)) ||
                    (v.DoingBusinessAs != null && v.DoingBusinessAs.ToLower().Contains(searchLower)));
            }
            
            var vendors = await query
                .OrderBy(v => v.VendorName)
                .Take(maxResults)
                .ToListAsync();
            
            return vendors.Select(MapToSearchResult).ToList();
        }

        public async Task<List<VendorSearchResult>> SearchAttorneysAsync(string searchTerm, string? attorneyType = null, int maxResults = 20)
        {
            var query = _context.VendorMasters
                .Include(v => v.Addresses)
                .Where(v => v.VendorStatus == 'Y' &&
                    (v.VendorType == VendorTypes.DefenseAttorney || 
                     v.VendorType == VendorTypes.PlaintiffAttorney ||
                     v.VendorType == "Plaintiff Attorneys" ||  // Legacy value
                     v.VendorType == "Attorney"));
            
            // Filter by specific attorney type
            if (!string.IsNullOrEmpty(attorneyType))
            {
                query = query.Where(v => v.VendorType == attorneyType || 
                    (attorneyType == "Plaintiff Attorneys" && v.VendorType == VendorTypes.PlaintiffAttorney));
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(v => 
                    (v.VendorName != null && v.VendorName.ToLower().Contains(searchLower)) ||
                    (v.DoingBusinessAs != null && v.DoingBusinessAs.ToLower().Contains(searchLower)) ||
                    (v.ContactName != null && v.ContactName.ToLower().Contains(searchLower)));
            }
            
            var vendors = await query
                .OrderBy(v => v.VendorName)
                .Take(maxResults)
                .ToListAsync();
            
            return vendors.Select(MapToSearchResult).ToList();
        }

        public async Task<List<VendorSearchResult>> SearchAuthoritiesAsync(string searchTerm, string? authorityType = null, int maxResults = 20)
        {
            var query = _context.VendorMasters
                .Include(v => v.Addresses)
                .Where(v => v.VendorStatus == 'Y' &&
                    (v.VendorType == VendorTypes.PoliceDepartment || 
                     v.VendorType == VendorTypes.FireDepartment ||
                     v.VendorType == "Fire Station"));  // Legacy value
            
            // Filter by specific authority type
            if (!string.IsNullOrEmpty(authorityType))
            {
                query = query.Where(v => v.VendorType == authorityType ||
                    (authorityType == "Fire Station" && v.VendorType == VendorTypes.FireDepartment));
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(v => 
                    (v.VendorName != null && v.VendorName.ToLower().Contains(searchLower)) ||
                    (v.DoingBusinessAs != null && v.DoingBusinessAs.ToLower().Contains(searchLower)));
            }
            
            var vendors = await query
                .OrderBy(v => v.VendorName)
                .Take(maxResults)
                .ToListAsync();
            
            return vendors.Select(MapToSearchResult).ToList();
        }

        public async Task<List<VendorSearchResult>> CheckDuplicateAsync(string name, string? vendorType = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<VendorSearchResult>();
            
            var nameLower = name.ToLower().Trim();
            
            var query = _context.VendorMasters
                .Include(v => v.Addresses)
                .Where(v => v.VendorStatus == 'Y' &&
                    v.VendorName != null && 
                    v.VendorName.ToLower().Trim() == nameLower);
            
            if (!string.IsNullOrEmpty(vendorType))
            {
                query = query.Where(v => v.VendorType == vendorType);
            }
            
            var vendors = await query.ToListAsync();
            
            // Also check for partial matches
            if (vendors.Count == 0)
            {
                var partialQuery = _context.VendorMasters
                    .Include(v => v.Addresses)
                    .Where(v => v.VendorStatus == 'Y' &&
                        v.VendorName != null && 
                        v.VendorName.ToLower().Contains(nameLower));
                
                if (!string.IsNullOrEmpty(vendorType))
                {
                    partialQuery = partialQuery.Where(v => v.VendorType == vendorType);
                }
                
                vendors = await partialQuery.Take(5).ToListAsync();
            }
            
            return vendors.Select(MapToSearchResult).ToList();
        }

        public async Task<VendorSearchResult?> GetVendorByIdAsync(long vendorId)
        {
            var vendor = await _context.VendorMasters
                .Include(v => v.Addresses)
                .FirstOrDefaultAsync(v => v.VendorId == vendorId);
            
            if (vendor == null)
                return null;
            
            return MapToSearchResult(vendor);
        }

        private VendorSearchResult MapToSearchResult(VendorMasterEntity vendor)
        {
            var mainAddress = vendor.Addresses?.FirstOrDefault(a => a.AddressType == 'M' && a.AddressStatus == 'Y')
                            ?? vendor.Addresses?.FirstOrDefault(a => a.AddressStatus == 'Y');
            
            return new VendorSearchResult
            {
                VendorId = vendor.VendorId,
                EntityId = vendor.LegacyEntityId ?? vendor.VendorId,  // For backward compatibility
                Name = vendor.VendorName ?? "",
                DoingBusinessAs = vendor.DoingBusinessAs,
                VendorType = vendor.VendorType ?? "",
                Phone = vendor.BusinessPhone ?? vendor.MobilePhone,
                Email = vendor.Email,
                FeinNumber = vendor.FEINNumber,
                ContactName = vendor.ContactName,
                IsActive = vendor.VendorStatus == 'Y',
                StreetAddress = mainAddress?.StreetAddress,
                AddressLine2 = mainAddress?.AddressLine2,
                City = mainAddress?.City,
                State = mainAddress?.State,
                ZipCode = mainAddress?.ZipCode
            };
        }
    }
}
