using ClaimsPortal.Data;
using ClaimsPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPortal.Services
{
    /// <summary>
    /// Request model for saving complete FNOL with all related data
    /// </summary>
    public class FnolSaveRequest
    {
        public Claim ClaimData { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public string? FnolNumber { get; set; }
    }

    /// <summary>
    /// Result model for previous reserves lookup
    /// </summary>
    public class PreviousReservesResult
    {
        public decimal ExpenseReserve { get; set; }
        public decimal IndemnityReserve { get; set; }
    }

    public interface IDatabaseClaimService
    {
        // FNOL Operations
        Task<Data.Fnol> CreateFnolAsync(Data.Fnol fnol);
        Task<Data.Fnol?> GetFnolAsync(long fnolId);
        Task<Data.Fnol?> GetFnolByNumberAsync(string fnolNumber);
        Task<List<Data.Fnol>> GetOpenFnolsAsync();
        Task<Data.Fnol> UpdateFnolAsync(Data.Fnol fnol);
        Task<Data.Fnol> SaveFnolAsDraftAsync(Data.Fnol fnol);
        Task<Data.Fnol> SaveFnolDraftWithDetailsAsync(FnolSaveRequest request);
        Task<Data.Fnol> SaveCompleteFnolAsync(FnolSaveRequest request);
        Task<string> GenerateFnolNumberAsync();
        Task<string> GenerateClaimNumberAsync();

        // Search Operations
        Task<List<Data.Fnol>> SearchFnolsAsync(string? fnolNumber, string? policyNumber, DateTime? dateOfLossFrom, DateTime? dateOfLossTo, char? status);
        Task<List<Data.Fnol>> SearchClaimsAsync(string? claimNumber, string? fnolNumber, string? policyNumber, DateTime? dateOfLossFrom, DateTime? dateOfLossTo, char? status);

        // Claim Detail Operations
        Task<Claim?> GetClaimWithDetailsAsync(string claimNumber);

        // Sub-Claim Operations
        Task<Data.SubClaim> CreateSubClaimAsync(Data.SubClaim subClaim);
        Task<List<Data.SubClaim>> GetSubClaimsAsync(long fnolId);
        Task<Data.SubClaim> UpdateSubClaimAsync(Data.SubClaim subClaim);
        
        // Sub-Claim Close/Reopen Operations
        Task CloseSubClaimWithDetailsAsync(int subClaimId, string reason, string remarks, string closedBy);
        Task ReopenSubClaimWithDetailsAsync(int subClaimId, string reason, string remarks, decimal expenseReserve, decimal indemnityReserve, string reopenedBy);
        Task<PreviousReservesResult> GetPreviousReservesAsync(int subClaimId);

        // Vehicle Operations
        Task<Data.Vehicle> AddVehicleAsync(Data.Vehicle vehicle);
        Task<List<Data.Vehicle>> GetVehiclesAsync(long fnolId);

        // Claimant Operations
        Task<Data.Claimant> AddClaimantAsync(Data.Claimant claimant);
        Task<List<Data.Claimant>> GetClaimantsAsync(long fnolId);
    }

    public class DatabaseClaimService : IDatabaseClaimService
    {
        private readonly ClaimsPortalDbContext _context;

        public DatabaseClaimService(ClaimsPortalDbContext context)
        {
            _context = context;
        }

        #region FNOL Operations

        public async Task<Data.Fnol> CreateFnolAsync(Data.Fnol fnol)
        {
            if (string.IsNullOrEmpty(fnol.FnolNumber))
            {
                fnol.FnolNumber = await GenerateFnolNumberAsync();
            }

            fnol.CreatedDate = DateTime.Now;
            fnol.CreatedTime = DateTime.Now.TimeOfDay;
            fnol.FnolStatus = 'D'; // Draft status

            _context.FNOLs.Add(fnol);
            // Backfill Vendor FK for any claimants that only have a legacy AttorneyEntityId
            var claimantEntries = _context.ChangeTracker.Entries<Data.Claimant>()
                .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var c in claimantEntries)
            {
                if (!c.AttorneyVendorId.HasValue && c.AttorneyEntityId.HasValue)
                {
                    var existingVendor = await _context.VendorMasters.FirstOrDefaultAsync(v => v.LegacyEntityId == c.AttorneyEntityId.Value);
                    if (existingVendor != null)
                    {
                        c.AttorneyVendorId = existingVendor.VendorId;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return fnol;
        }

        public async Task<Data.Fnol?> GetFnolAsync(long fnolId)
        {
            return await _context.FNOLs.FirstOrDefaultAsync(f => f.FnolId == fnolId);
        }

        public async Task<Data.Fnol?> GetFnolByNumberAsync(string fnolNumber)
        {
            // Try to find by FNOL number first
            var fnol = await _context.FNOLs.FirstOrDefaultAsync(f => f.FnolNumber == fnolNumber);
            
            // If not found, try by claim number
            if (fnol == null)
            {
                fnol = await _context.FNOLs.FirstOrDefaultAsync(f => f.ClaimNumber == fnolNumber);
            }

            return fnol;
        }

        public async Task<List<Data.Fnol>> GetOpenFnolsAsync()
        {
            return await _context.FNOLs
                .Where(f => f.FnolStatus == 'O' || f.FnolStatus == 'D')
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<Data.Fnol> UpdateFnolAsync(Data.Fnol fnol)
        {
            fnol.ModifiedDate = DateTime.Now;
            fnol.ModifiedTime = DateTime.Now.TimeOfDay;
            _context.FNOLs.Update(fnol);
            await _context.SaveChangesAsync();
            return fnol;
        }

        public async Task<Data.Fnol> SaveFnolAsDraftAsync(Data.Fnol fnol)
        {
            // Check if FNOL already exists
            var existingFnol = await _context.FNOLs
                .FirstOrDefaultAsync(f => f.FnolNumber == fnol.FnolNumber);

            if (existingFnol != null)
            {
                // Update existing FNOL
                existingFnol.PolicyNumber = fnol.PolicyNumber;
                existingFnol.PolicyEffectiveDate = fnol.PolicyEffectiveDate;
                existingFnol.PolicyExpirationDate = fnol.PolicyExpirationDate;
                existingFnol.PolicyCancelDate = fnol.PolicyCancelDate;
                existingFnol.PolicyStatus = fnol.PolicyStatus;
                existingFnol.InsuredName = fnol.InsuredName;
                existingFnol.InsuredPhone = fnol.InsuredPhone;
                existingFnol.RenewalNumber = fnol.RenewalNumber;
                existingFnol.DateOfLoss = fnol.DateOfLoss;
                existingFnol.TimeOfLoss = fnol.TimeOfLoss;
                existingFnol.ReportDate = fnol.ReportDate;
                existingFnol.ReportTime = fnol.ReportTime;
                existingFnol.LossLocation = fnol.LossLocation;
                existingFnol.LossLocation2 = fnol.LossLocation2;
                existingFnol.CauseOfLoss = fnol.CauseOfLoss;
                existingFnol.WeatherConditions = fnol.WeatherConditions;
                existingFnol.LossDescription = fnol.LossDescription;
                existingFnol.HasInjury = fnol.HasInjury;
                existingFnol.HasPropertyDamage = fnol.HasPropertyDamage;
                existingFnol.HasOtherVehiclesInvolved = fnol.HasOtherVehiclesInvolved;
                existingFnol.ReportedBy = fnol.ReportedBy;
                existingFnol.ReportedByName = fnol.ReportedByName;
                existingFnol.ReportedByPhone = fnol.ReportedByPhone;
                existingFnol.ReportedByEmail = fnol.ReportedByEmail;
                existingFnol.ReportingMethod = fnol.ReportingMethod;
                existingFnol.ModifiedDate = DateTime.Now;
                existingFnol.ModifiedTime = DateTime.Now.TimeOfDay;
                existingFnol.ModifiedBy = fnol.CreatedBy;

                _context.FNOLs.Update(existingFnol);
                await _context.SaveChangesAsync();
                return existingFnol;
            }
            else
            {
                // Create new FNOL
                fnol.FnolStatus = 'D'; // Draft
                fnol.CreatedDate = DateTime.Now;
                fnol.CreatedTime = DateTime.Now.TimeOfDay;
                _context.FNOLs.Add(fnol);
                await _context.SaveChangesAsync();
                return fnol;
            }
        }

        public async Task<Data.Fnol> SaveFnolDraftWithDetailsAsync(FnolSaveRequest request)
        {
            // Get or create FNOL
            Data.Fnol fnol;
            if (!string.IsNullOrEmpty(request.FnolNumber))
            {
                fnol = await _context.FNOLs.FirstOrDefaultAsync(f => f.FnolNumber == request.FnolNumber)
                    ?? new Data.Fnol { FnolNumber = request.FnolNumber };
            }
            else
            {
                fnol = new Data.Fnol
                {
                    FnolNumber = await GenerateFnolNumberAsync()
                };
            }

            // Map claim data to FNOL
            var claim = request.ClaimData;
            PolicyMapper.ApplyPolicyToFnol(claim.PolicyInfo, fnol);
            fnol.DateOfLoss = claim.LossDetails?.DateOfLoss ?? DateTime.Now;
            fnol.TimeOfLoss = claim.LossDetails?.TimeOfLoss.ToTimeSpan();
            fnol.ReportDate = claim.LossDetails?.ReportDate ?? DateTime.Now;
            fnol.ReportTime = claim.LossDetails?.ReportTime.ToTimeSpan();
            fnol.LossLocation = claim.LossDetails?.Location ?? "";
            fnol.LossLocation2 = claim.LossDetails?.Location2;
            fnol.CauseOfLoss = claim.LossDetails?.CauseOfLoss;
            fnol.WeatherConditions = claim.LossDetails?.WeatherCondition;
            fnol.LossDescription = claim.LossDetails?.LossDescription;
            fnol.HasInjury = claim.LossDetails?.HasInjuries ?? false;
            fnol.HasPropertyDamage = claim.LossDetails?.HasPropertyDamage ?? false;
            fnol.HasOtherVehiclesInvolved = claim.LossDetails?.HasOtherVehiclesInvolved ?? false;
            fnol.ReportedBy = claim.LossDetails?.ReportedBy;
            fnol.ReportedByName = claim.LossDetails?.ReportedByName;
            fnol.ReportedByPhone = claim.LossDetails?.ReportedByPhone;
            fnol.ReportedByEmail = claim.LossDetails?.ReportedByEmail;
            fnol.ReportingMethod = claim.LossDetails?.ReportingMethod;

            // If the report was made by 'Other', create an EntityMaster + AddressMaster and link it
            if (string.Equals(claim.LossDetails?.ReportedBy, "Other", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(claim.LossDetails?.ReportedByName))
            {
                var entityCreator = new EntityCreationService(_context);
                fnol.ReportedByEntityId = await entityCreator.CreateReportedByEntityAsync(claim.LossDetails, request.CreatedBy);
            }

            // Reported-by entity creation moved to EntityCreationService to avoid duplication

            // Insured information
            fnol.InsuredName = claim.InsuredParty?.Name ?? claim.PolicyInfo?.InsuredName;
            fnol.InsuredPhone = claim.InsuredParty?.PhoneNumber ?? claim.PolicyInfo?.PhoneNumber;
            fnol.InsuredEmail = claim.InsuredParty?.Email ?? claim.PolicyInfo?.Email;
            fnol.InsuredDoingBusinessAs = claim.InsuredParty?.DoingBusinessAs ?? claim.PolicyInfo?.DoingBusinessAs;
            fnol.InsuredBusinessType = claim.InsuredParty?.BusinessType;
            fnol.InsuredAddress = claim.InsuredParty?.Address?.StreetAddress ?? claim.PolicyInfo?.Address;
            fnol.InsuredAddress2 = claim.InsuredParty?.Address?.AddressLine2 ?? claim.PolicyInfo?.Address2;
            fnol.InsuredCity = claim.InsuredParty?.Address?.City ?? claim.PolicyInfo?.City;
            fnol.InsuredState = claim.InsuredParty?.Address?.State ?? claim.PolicyInfo?.State;
            fnol.InsuredZipCode = claim.InsuredParty?.Address?.ZipCode ?? claim.PolicyInfo?.ZipCode;
            fnol.InsuredFeinSsNumber = claim.InsuredParty?.FeinSsNumber;
            fnol.InsuredLicenseNumber = claim.InsuredParty?.LicenseNumber;
            fnol.InsuredLicenseState = claim.InsuredParty?.LicenseState;
            fnol.InsuredDateOfBirth = claim.InsuredParty?.DateOfBirth;

            fnol.FnolStatus = 'D'; // Draft
            fnol.CreatedBy = request.CreatedBy;

            if (fnol.FnolId == 0)
            {
                fnol.CreatedDate = DateTime.Now;
                fnol.CreatedTime = DateTime.Now.TimeOfDay;
                _context.FNOLs.Add(fnol);
            }
            else
            {
                fnol.ModifiedDate = DateTime.Now;
                fnol.ModifiedTime = DateTime.Now.TimeOfDay;
                fnol.ModifiedBy = request.CreatedBy;
                _context.FNOLs.Update(fnol);
            }

            await _context.SaveChangesAsync();

            // Clear existing related entities before re-saving (for draft updates)
            await ClearRelatedEntitiesAsync(fnol.FnolId);

            // Save related entities (vehicles, claimants, sub-claims, witnesses, authorities, etc.)
            await SaveRelatedEntitiesAsync(fnol, claim, request.CreatedBy);

            return fnol;
        }

        public async Task<Data.Fnol> SaveCompleteFnolAsync(FnolSaveRequest request)
        {
            // Get or create FNOL
            Data.Fnol fnol;
            if (!string.IsNullOrEmpty(request.FnolNumber))
            {
                fnol = await _context.FNOLs.FirstOrDefaultAsync(f => f.FnolNumber == request.FnolNumber)
                    ?? new Data.Fnol { FnolNumber = request.FnolNumber };
            }
            else
            {
                fnol = new Data.Fnol
                {
                    FnolNumber = await GenerateFnolNumberAsync()
                };
            }

            // Generate Claim Number if not exists
            if (string.IsNullOrEmpty(fnol.ClaimNumber))
            {
                fnol.ClaimNumber = await GenerateClaimNumberAsync();
            }

            // Map claim data to FNOL
            var claim = request.ClaimData;
            PolicyMapper.ApplyPolicyToFnol(claim.PolicyInfo, fnol);
            fnol.DateOfLoss = claim.LossDetails?.DateOfLoss ?? DateTime.Now;
            fnol.TimeOfLoss = claim.LossDetails?.TimeOfLoss.ToTimeSpan();
            fnol.ReportDate = claim.LossDetails?.ReportDate ?? DateTime.Now;
            fnol.ReportTime = claim.LossDetails?.ReportTime.ToTimeSpan();
            fnol.LossLocation = claim.LossDetails?.Location ?? "";
            fnol.LossLocation2 = claim.LossDetails?.Location2;
            fnol.CauseOfLoss = claim.LossDetails?.CauseOfLoss;
            fnol.WeatherConditions = claim.LossDetails?.WeatherCondition;
            fnol.LossDescription = claim.LossDetails?.LossDescription;
            fnol.HasInjury = claim.LossDetails?.HasInjuries ?? false;
            fnol.HasPropertyDamage = claim.LossDetails?.HasPropertyDamage ?? false;
            fnol.HasOtherVehiclesInvolved = claim.LossDetails?.HasOtherVehiclesInvolved ?? false;
            fnol.ReportedBy = claim.LossDetails?.ReportedBy;
            fnol.ReportedByName = claim.LossDetails?.ReportedByName;
            fnol.ReportedByPhone = claim.LossDetails?.ReportedByPhone;
            fnol.ReportedByEmail = claim.LossDetails?.ReportedByEmail;
            fnol.ReportingMethod = claim.LossDetails?.ReportingMethod;

            // Insured information
            fnol.InsuredName = claim.InsuredParty?.Name ?? claim.PolicyInfo?.InsuredName;
            fnol.InsuredPhone = claim.InsuredParty?.PhoneNumber ?? claim.PolicyInfo?.PhoneNumber;
            fnol.InsuredEmail = claim.InsuredParty?.Email ?? claim.PolicyInfo?.Email;
            fnol.InsuredDoingBusinessAs = claim.InsuredParty?.DoingBusinessAs ?? claim.PolicyInfo?.DoingBusinessAs;
            fnol.InsuredBusinessType = claim.InsuredParty?.BusinessType;
            fnol.InsuredAddress = claim.InsuredParty?.Address?.StreetAddress ?? claim.PolicyInfo?.Address;
            fnol.InsuredAddress2 = claim.InsuredParty?.Address?.AddressLine2 ?? claim.PolicyInfo?.Address2;
            fnol.InsuredCity = claim.InsuredParty?.Address?.City ?? claim.PolicyInfo?.City;
            fnol.InsuredState = claim.InsuredParty?.Address?.State ?? claim.PolicyInfo?.State;
            fnol.InsuredZipCode = claim.InsuredParty?.Address?.ZipCode ?? claim.PolicyInfo?.ZipCode;
            fnol.InsuredFeinSsNumber = claim.InsuredParty?.FeinSsNumber;
            fnol.InsuredLicenseNumber = claim.InsuredParty?.LicenseNumber;
            fnol.InsuredLicenseState = claim.InsuredParty?.LicenseState;
            fnol.InsuredDateOfBirth = claim.InsuredParty?.DateOfBirth;

            fnol.FnolStatus = 'O'; // Open
            fnol.CreatedBy = request.CreatedBy;

            if (fnol.FnolId == 0)
            {
                fnol.CreatedDate = DateTime.Now;
                fnol.CreatedTime = DateTime.Now.TimeOfDay;
                _context.FNOLs.Add(fnol);
            }
            else
            {
                fnol.ModifiedDate = DateTime.Now;
                fnol.ModifiedTime = DateTime.Now.TimeOfDay;
                fnol.ModifiedBy = request.CreatedBy;
                _context.FNOLs.Update(fnol);
            }

            await _context.SaveChangesAsync();

            // Clear existing related entities before re-saving (prevents duplicates when updating)
            await ClearRelatedEntitiesAsync(fnol.FnolId);

            // Save related entities (vehicles, claimants, sub-claims, etc.)
            await SaveRelatedEntitiesAsync(fnol, claim, request.CreatedBy);

            return fnol;
        }

        private async Task ClearRelatedEntitiesAsync(long fnolId)
        {
            // Clear existing witnesses
            var existingWitnesses = await _context.FnolWitnesses.Where(w => w.FnolId == fnolId).ToListAsync();
            _context.FnolWitnesses.RemoveRange(existingWitnesses);

            // Clear existing authorities
            var existingAuthorities = await _context.FnolAuthorities.Where(a => a.FnolId == fnolId).ToListAsync();
            _context.FnolAuthorities.RemoveRange(existingAuthorities);

            // Clear existing vehicles
            var existingVehicles = await _context.Vehicles.Where(v => v.FnolId == fnolId).ToListAsync();
            _context.Vehicles.RemoveRange(existingVehicles);

            // Clear existing claimants
            var existingClaimants = await _context.Claimants.Where(c => c.FnolId == fnolId).ToListAsync();
            _context.Claimants.RemoveRange(existingClaimants);

            // Clear existing property damages
            var existingPropertyDamages = await _context.FnolPropertyDamages.Where(pd => pd.FnolId == fnolId).ToListAsync();
            _context.FnolPropertyDamages.RemoveRange(existingPropertyDamages);

            // Clear existing sub-claims
            var existingSubClaims = await _context.SubClaims.Where(s => s.FnolId == fnolId).ToListAsync();
            _context.SubClaims.RemoveRange(existingSubClaims);

            await _context.SaveChangesAsync();
        }

        private async Task SaveRelatedEntitiesAsync(Data.Fnol fnol, Claim claim, string createdBy)
        {
            var claimantEntityMap = new Dictionary<string, long>();

            // Save witnesses
            foreach (var witness in claim.LossDetails.Witnesses)
            {
                var entity = new Data.EntityMaster 
                { 
                    EntityType = 'I', 
                    PartyType = "Witness", 
                    EntityGroupCode = "Witness", 
                    EntityName = witness.Name, 
                    HomeBusinessPhone = witness.PhoneNumber, 
                    Email = witness.Email, 
                    EntityStatus = 'Y', 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                _context.EntityMasters.Add(entity);
                await _context.SaveChangesAsync();
                
                // Save witness address if provided
                if (witness.Address != null && witness.Address.HasAnyAddress)
                {
                    var addr = new Data.AddressMaster
                    {
                        EntityId = entity.EntityId,
                        AddressType = 'M',
                        StreetAddress = witness.Address.StreetAddress,
                        Apt = witness.Address.AddressLine2,
                        City = witness.Address.City,
                        State = witness.Address.State,
                        ZipCode = witness.Address.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    };

                    _context.Addresses.Add(addr);
                    // Persist immediately to ensure City/State/Zip saved (addresses were reported missing)
                    await _context.SaveChangesAsync();
                }
                
                _context.FnolWitnesses.Add(new Data.FnolWitness 
                { 
                    FnolId = fnol.FnolId, 
                    EntityId = entity.EntityId, 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                });
            }

            // Save authorities
            foreach (var authority in claim.LossDetails.AuthoritiesNotified)
            {
                // If UI provided an explicit VendorId (selected from Vendor Master), use it directly
                if (authority.VendorId.HasValue)
                {
                    var vendor = await _context.VendorMasters
                        .Include(v => v.Addresses)
                        .FirstOrDefaultAsync(v => v.VendorId == authority.VendorId.Value);
                    if (vendor != null)
                    {
                        long entityId;
                        if (vendor.LegacyEntityId.HasValue && vendor.LegacyEntityId.Value > 0)
                        {
                            entityId = vendor.LegacyEntityId.Value;
                        }
                        else
                        {
                            // Create a minimal EntityMaster record for this vendor for backward compatibility
                            var vendorEntity = new Data.EntityMaster
                            {
                                EntityType = 'B',
                                PartyType = vendor.VendorType ?? authority.AuthorityName,
                                EntityGroupCode = "Vendor",
                                EntityName = vendor.VendorName,
                                ContactName = vendor.ContactName,
                                HomeBusinessPhone = vendor.BusinessPhone ?? vendor.MobilePhone,
                                Email = vendor.Email,
                                EntityStatus = 'Y',
                                CreatedDate = DateTime.Now,
                                CreatedBy = createdBy
                            };
                            _context.EntityMasters.Add(vendorEntity);
                            await _context.SaveChangesAsync();

                            // Update VendorMaster.LegacyEntityId for future reference
                            vendor.LegacyEntityId = vendorEntity.EntityId;
                            _context.VendorMasters.Update(vendor);
                            await _context.SaveChangesAsync();

                            entityId = vendorEntity.EntityId;
                        }

                        _context.FnolAuthorities.Add(new Data.FnolAuthority
                        {
                            FnolId = fnol.FnolId,
                            EntityId = entityId,
                            VendorId = vendor.VendorId,
                            AuthorityType = authority.AuthorityName,
                            ReportNumber = authority.ReportNumber,
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        });

                        // No EntityMaster/AddressMaster created for vendor-backed authorities
                        await _context.SaveChangesAsync();
                        continue;
                    }
                    // If vendor id was provided but not found, fall through to other logic
                }
                // If no explicit VendorId, attempt to match by vendor name (case-insensitive LIKE)
                var nameToMatch = (authority.Name ?? string.Empty).Trim();
                var existingVendor = string.IsNullOrEmpty(nameToMatch) ? null :
                    await _context.VendorMasters
                        .Include(v => v.Addresses)
                        .FirstOrDefaultAsync(v => EF.Functions.Like(v.VendorName ?? "", $"%{nameToMatch}%"));
                if (existingVendor != null)
                {
                    long entityId;
                    if (existingVendor.LegacyEntityId.HasValue && existingVendor.LegacyEntityId.Value > 0)
                    {
                        entityId = existingVendor.LegacyEntityId.Value;
                    }
                    else
                    {
                        var vendorEntity = new Data.EntityMaster
                        {
                            EntityType = 'B',
                            PartyType = existingVendor.VendorType ?? authority.AuthorityName,
                            EntityGroupCode = "Vendor",
                            EntityName = existingVendor.VendorName,
                            ContactName = existingVendor.ContactName,
                            HomeBusinessPhone = existingVendor.BusinessPhone ?? existingVendor.MobilePhone,
                            Email = existingVendor.Email,
                            EntityStatus = 'Y',
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        };
                        _context.EntityMasters.Add(vendorEntity);
                        await _context.SaveChangesAsync();
                        
                        existingVendor.LegacyEntityId = vendorEntity.EntityId;
                        _context.VendorMasters.Update(existingVendor);
                        await _context.SaveChangesAsync();
                        
                        entityId = vendorEntity.EntityId;
                    }

                    _context.FnolAuthorities.Add(new Data.FnolAuthority
                    {
                        FnolId = fnol.FnolId,
                        EntityId = entityId,
                        VendorId = existingVendor.VendorId,
                        AuthorityType = authority.AuthorityName,
                        ReportNumber = authority.ReportNumber,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    });

                    await _context.SaveChangesAsync();
                    continue;
                }

                var entity = new Data.EntityMaster 
                { 
                    EntityType = 'B', 
                    PartyType = authority.AuthorityName, 
                    EntityGroupCode = "Vendor", 
                    EntityName = authority.Name, 
                    EntityStatus = 'Y', 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                _context.EntityMasters.Add(entity);
                await _context.SaveChangesAsync();
                
                // Save authority address if provided
                if (authority.Address != null && authority.Address.HasAnyAddress)
                {
                    var authAddr = new Data.AddressMaster
                    {
                        EntityId = entity.EntityId,
                        AddressType = 'M',
                        StreetAddress = authority.Address.StreetAddress,
                        Apt = authority.Address.AddressLine2,
                        City = authority.Address.City,
                        State = authority.Address.State,
                        ZipCode = authority.Address.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    };

                    _context.Addresses.Add(authAddr);
                    // Persist immediately to avoid missing city/state/zip fields
                    await _context.SaveChangesAsync();
                }
                
                _context.FnolAuthorities.Add(new Data.FnolAuthority 
                { 
                    FnolId = fnol.FnolId, 
                    EntityId = entity.EntityId, 
                    AuthorityType = authority.AuthorityName, 
                    ReportNumber = authority.ReportNumber, 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                });
            }

            // Save driver attorney if provided. Prefer VendorMaster records; create vendor and legacy EntityMaster when manually entered.
            long? driverAttorneyVendorId = null;
            long? driverAttorneyEntityId = null;
            if (claim.DriverAttorney != null && !string.IsNullOrEmpty(claim.DriverAttorney.Name))
            {
                // If UI passed a Vendor id (selected from VendorMaster)
                if (claim.DriverAttorney.VendorEntityId.HasValue)
                {
                    var vendor = await _context.VendorMasters
                        .Include(v => v.Addresses)
                        .FirstOrDefaultAsync(v => v.VendorId == claim.DriverAttorney.VendorEntityId.Value);
                    if (vendor != null)
                    {
                        driverAttorneyVendorId = vendor.VendorId;
                        if (vendor.LegacyEntityId.HasValue && vendor.LegacyEntityId.Value > 0)
                        {
                            driverAttorneyEntityId = vendor.LegacyEntityId.Value;
                        }
                        else
                        {
                            var attorneyEntity = new Data.EntityMaster
                            {
                                EntityType = 'B',
                                PartyType = "Plaintiff Attorney",
                                EntityGroupCode = "Vendor",
                                EntityName = vendor.VendorName,
                                ContactName = vendor.ContactName,
                                HomeBusinessPhone = vendor.BusinessPhone ?? vendor.MobilePhone,
                                Email = vendor.Email,
                                EntityStatus = 'Y',
                                CreatedDate = DateTime.Now,
                                CreatedBy = createdBy
                            };
                            _context.EntityMasters.Add(attorneyEntity);
                            await _context.SaveChangesAsync();

                            // Link back to vendor for future lookups
                            vendor.LegacyEntityId = attorneyEntity.EntityId;
                            _context.VendorMasters.Update(vendor);
                            await _context.SaveChangesAsync();

                            driverAttorneyEntityId = attorneyEntity.EntityId;
                        }
                    }
                }
                else
                {
                    // No VendorId provided -> create new VendorMaster record for this attorney, then create legacy EntityMaster
                    var newVendor = new Data.VendorMasterEntity
                    {
                        VendorName = claim.DriverAttorney.FirmName ?? claim.DriverAttorney.Name,
                        ContactName = claim.DriverAttorney.Name,
                        BusinessPhone = claim.DriverAttorney.PhoneNumber,
                        Email = claim.DriverAttorney.Email,
                        VendorStatus = 'Y',
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    };
                    _context.VendorMasters.Add(newVendor);
                    await _context.SaveChangesAsync();

                    driverAttorneyVendorId = newVendor.VendorId;

                    var attorneyEntity = new Data.EntityMaster
                    {
                        EntityType = 'B',
                        PartyType = "Plaintiff Attorney",
                        EntityGroupCode = "Vendor",
                        EntityName = newVendor.VendorName,
                        ContactName = newVendor.ContactName,
                        HomeBusinessPhone = newVendor.BusinessPhone,
                        Email = newVendor.Email,
                        EntityStatus = 'Y',
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    };
                    _context.EntityMasters.Add(attorneyEntity);
                    await _context.SaveChangesAsync();

                    // Link legacy entity back to vendor
                    newVendor.LegacyEntityId = attorneyEntity.EntityId;
                    _context.VendorMasters.Update(newVendor);
                    await _context.SaveChangesAsync();

                    driverAttorneyEntityId = attorneyEntity.EntityId;
                }
            }

            // Save insured vehicle and driver
            long? driverEntityId = null;
            if (!string.IsNullOrEmpty(claim.Driver?.Name))
            {
                var driverEntity = new Data.EntityMaster 
                { 
                    EntityType = 'I', 
                    PartyType = "Driver", 
                    EntityGroupCode = "Claimant", 
                    EntityName = claim.Driver.Name, 
                    LicenseNumber = claim.Driver.LicenseNumber, 
                    LicenseState = claim.Driver.LicenseState, 
                    HomeBusinessPhone = claim.Driver.PhoneNumber,
                    Email = claim.Driver.Email,
                    DateOfBirth = claim.Driver.DateOfBirth != default ? claim.Driver.DateOfBirth : null,
                    EntityStatus = 'Y', 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                _context.EntityMasters.Add(driverEntity);
                await _context.SaveChangesAsync();
                driverEntityId = driverEntity.EntityId;
                claimantEntityMap[claim.Driver.Name] = driverEntityId.Value;
                
                // Save driver address if provided
                if (claim.Driver.Address != null && claim.Driver.Address.HasAnyAddress)
                {
                    _context.Addresses.Add(new Data.AddressMaster
                    {
                        EntityId = driverEntity.EntityId,
                        AddressType = 'M',
                        StreetAddress = claim.Driver.Address.StreetAddress,
                        Apt = claim.Driver.Address.AddressLine2,
                        City = claim.Driver.Address.City,
                        State = claim.Driver.Address.State,
                        ZipCode = claim.Driver.Address.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    });
                }

                // Create claimant record for driver (always, for tracking purposes)
                var driverClaimant = new Data.Claimant 
                { 
                    FnolId = fnol.FnolId, 
                    ClaimNumber = fnol.ClaimNumber, 
                    ClaimantEntityId = driverEntityId, 
                    ClaimantName = claim.Driver.Name, 
                    ClaimantType = "IVD",
                    HasInjury = claim.DriverInjury != null && !string.IsNullOrEmpty(claim.DriverInjury.InjuryType),
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                
                // Add driver injury details if injured
                if (claim.DriverInjury != null && !string.IsNullOrEmpty(claim.DriverInjury.InjuryType))
                {
                    driverClaimant.InjuryType = claim.DriverInjury.InjuryType;
                    driverClaimant.InjuryDescription = claim.DriverInjury.InjuryDescription;
                    driverClaimant.InjurySeverity = claim.DriverInjury.SeverityLevel.ToString();
                    driverClaimant.IsFatality = claim.DriverInjury.IsFatality;
                    driverClaimant.IsHospitalized = claim.DriverInjury.WasTakenToHospital;
                    driverClaimant.HospitalName = claim.DriverInjury.HospitalName;
                    driverClaimant.HospitalStreetAddress = claim.DriverInjury.HospitalStreetAddress;
                    driverClaimant.HospitalCity = claim.DriverInjury.HospitalCity;
                    driverClaimant.HospitalState = claim.DriverInjury.HospitalState;
                    driverClaimant.HospitalZipCode = claim.DriverInjury.HospitalZipCode;
                    driverClaimant.TreatingPhysician = claim.DriverInjury.TreatingPhysician;
                }
                
                // Add driver attorney reference if provided (store vendor FK and legacy entity FK)
                if (driverAttorneyVendorId.HasValue || driverAttorneyEntityId.HasValue)
                {
                    driverClaimant.IsAttorneyRepresented = true;
                    if (driverAttorneyVendorId.HasValue) driverClaimant.AttorneyVendorId = driverAttorneyVendorId;
                    if (driverAttorneyEntityId.HasValue) driverClaimant.AttorneyEntityId = driverAttorneyEntityId;
                }
                
                _context.Claimants.Add(driverClaimant);
            }

            // Save insured vehicle
            if (claim.InsuredVehicle != null && !string.IsNullOrEmpty(claim.InsuredVehicle.Vin))
            {
                _context.Vehicles.Add(new Data.Vehicle 
                { 
                    FnolId = fnol.FnolId, 
                    ClaimNumber = fnol.ClaimNumber, 
                    VehicleParty = "IVD", 
                    VIN = claim.InsuredVehicle.Vin, 
                    Make = claim.InsuredVehicle.Make, 
                    Model = claim.InsuredVehicle.Model, 
                    Year = claim.InsuredVehicle.Year, 
                    PlateNumber = claim.InsuredVehicle.PlateNumber,
                    PlateState = claim.InsuredVehicle.PlateState,
                    IsVehicleDamaged = claim.InsuredVehicle.IsDamaged,
                    IsDrivable = claim.InsuredVehicle.IsDrivable,
                    WasTowed = claim.InsuredVehicle.WasTowed,
                    IsInStorage = claim.InsuredVehicle.InStorage,
                    StorageLocation = claim.InsuredVehicle.StorageLocation,
                    HasDashCam = claim.InsuredVehicle.HasDashCam,
                    DamageDetails = claim.InsuredVehicle.DamageDetails,
                    DidVehicleRollOver = claim.InsuredVehicle.DidVehicleRollOver,
                    HadWaterDamage = claim.InsuredVehicle.HadWaterDamage,
                    // map UI VehicleInfo properties to database Vehicle fields
                    HeadlightsWereOn = claim.InsuredVehicle.AreHeadlightsOn,
                    AirbagDeployed = claim.InsuredVehicle.DidAirbagDeploy,
                    DriverEntityId = driverEntityId, 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                });
            }

            // Save passengers with full details
            foreach (var passenger in claim.Passengers)
            {
                var entity = new Data.EntityMaster 
                { 
                    EntityType = 'I', 
                    PartyType = "Passenger", 
                    EntityGroupCode = "Claimant", 
                    EntityName = passenger.Name, 
                    HomeBusinessPhone = passenger.PhoneNumber, 
                    Email = passenger.Email, 
                    EntityStatus = 'Y', 
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                _context.EntityMasters.Add(entity);
                await _context.SaveChangesAsync();
                claimantEntityMap[passenger.Name] = entity.EntityId;
                
                // Save passenger address if provided
                if (passenger.Address != null && passenger.Address.HasAnyAddress)
                {
                    _context.Addresses.Add(new Data.AddressMaster
                    {
                        EntityId = entity.EntityId,
                        AddressType = 'M',
                        StreetAddress = passenger.Address.StreetAddress,
                        Apt = passenger.Address.AddressLine2,
                        City = passenger.Address.City,
                        State = passenger.Address.State,
                        ZipCode = passenger.Address.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    });
                }
                
                // Save passenger attorney if provided (prefer VendorMaster)
                long? passengerAttorneyVendorId = null;
                long? passengerAttorneyEntityId = null;
                if (passenger.HasAttorney && passenger.AttorneyInfo != null && !string.IsNullOrEmpty(passenger.AttorneyInfo.Name))
                {
                    if (passenger.AttorneyInfo.VendorEntityId.HasValue)
                    {
                        var vendor = await _context.VendorMasters
                            .Include(v => v.Addresses)
                            .FirstOrDefaultAsync(v => v.VendorId == passenger.AttorneyInfo.VendorEntityId.Value);
                        if (vendor != null)
                        {
                            passengerAttorneyVendorId = vendor.VendorId;
                            if (vendor.LegacyEntityId.HasValue && vendor.LegacyEntityId.Value > 0)
                                passengerAttorneyEntityId = vendor.LegacyEntityId.Value;
                            else
                            {
                                var attorneyEntity = new Data.EntityMaster
                                {
                                    EntityType = 'B',
                                    PartyType = "Plaintiff Attorney",
                                    EntityGroupCode = "Vendor",
                                    EntityName = vendor.VendorName,
                                    ContactName = vendor.ContactName,
                                    HomeBusinessPhone = vendor.BusinessPhone ?? vendor.MobilePhone,
                                    Email = vendor.Email,
                                    EntityStatus = 'Y',
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = createdBy
                                };
                                _context.EntityMasters.Add(attorneyEntity);
                                await _context.SaveChangesAsync();

                                vendor.LegacyEntityId = attorneyEntity.EntityId;
                                _context.VendorMasters.Update(vendor);
                                await _context.SaveChangesAsync();

                                passengerAttorneyEntityId = attorneyEntity.EntityId;
                            }
                        }
                    }
                    else
                    {
                        // Create VendorMaster record for manually entered attorney
                        var newVendor = new Data.VendorMasterEntity
                        {
                            VendorName = passenger.AttorneyInfo.FirmName ?? passenger.AttorneyInfo.Name,
                            ContactName = passenger.AttorneyInfo.Name,
                            BusinessPhone = passenger.AttorneyInfo.PhoneNumber,
                            Email = passenger.AttorneyInfo.Email,
                            VendorStatus = 'Y',
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        };
                        _context.VendorMasters.Add(newVendor);
                        await _context.SaveChangesAsync();

                        passengerAttorneyVendorId = newVendor.VendorId;

                        var attorneyEntity = new Data.EntityMaster
                        {
                            EntityType = 'B',
                            PartyType = "Plaintiff Attorney",
                            EntityGroupCode = "Vendor",
                            EntityName = newVendor.VendorName,
                            ContactName = newVendor.ContactName,
                            HomeBusinessPhone = newVendor.BusinessPhone,
                            Email = newVendor.Email,
                            EntityStatus = 'Y',
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        };
                        _context.EntityMasters.Add(attorneyEntity);
                        await _context.SaveChangesAsync();

                        newVendor.LegacyEntityId = attorneyEntity.EntityId;
                        _context.VendorMasters.Update(newVendor);
                        await _context.SaveChangesAsync();

                        passengerAttorneyEntityId = attorneyEntity.EntityId;
                    }
                }
                
                // Create claimant record with full injury and attorney details
                var passengerClaimant = new Data.Claimant 
                { 
                    FnolId = fnol.FnolId, 
                    ClaimNumber = fnol.ClaimNumber, 
                    ClaimantEntityId = entity.EntityId, 
                    ClaimantName = passenger.Name, 
                    ClaimantType = "IVP", 
                    HasInjury = passenger.WasInjured, 
                    IsAttorneyRepresented = (passengerAttorneyVendorId.HasValue || passengerAttorneyEntityId.HasValue),
                    AttorneyVendorId = passengerAttorneyVendorId,
                    AttorneyEntityId = passengerAttorneyEntityId,
                    CreatedDate = DateTime.Now, 
                    CreatedBy = createdBy 
                };
                
                // Add injury details if injured
                if (passenger.WasInjured && passenger.InjuryInfo != null)
                {
                    passengerClaimant.InjuryType = passenger.InjuryInfo.InjuryType;
                    passengerClaimant.InjuryDescription = passenger.InjuryInfo.InjuryDescription;
                    passengerClaimant.InjurySeverity = passenger.InjuryInfo.SeverityLevel.ToString();
                    passengerClaimant.IsFatality = passenger.InjuryInfo.IsFatality;
                    passengerClaimant.IsHospitalized = passenger.InjuryInfo.WasTakenToHospital;
                    passengerClaimant.HospitalName = passenger.InjuryInfo.HospitalName;
                    passengerClaimant.HospitalStreetAddress = passenger.InjuryInfo.HospitalStreetAddress;
                    passengerClaimant.HospitalCity = passenger.InjuryInfo.HospitalCity;
                    passengerClaimant.HospitalState = passenger.InjuryInfo.HospitalState;
                    passengerClaimant.HospitalZipCode = passenger.InjuryInfo.HospitalZipCode;
                    passengerClaimant.TreatingPhysician = passenger.InjuryInfo.TreatingPhysician;
                }
                
                _context.Claimants.Add(passengerClaimant);
            }

            // Save third parties with full details
            foreach (var tp in claim.ThirdParties)
            {
                // Save third party attorney if provided (prefer VendorMaster)
                long? tpAttorneyVendorId = null;
                long? tpAttorneyEntityId = null;
                if (tp.HasAttorney && tp.AttorneyInfo != null && !string.IsNullOrEmpty(tp.AttorneyInfo.Name))
                {
                    if (tp.AttorneyInfo.VendorEntityId.HasValue)
                    {
                        var vendor = await _context.VendorMasters
                            .Include(v => v.Addresses)
                            .FirstOrDefaultAsync(v => v.VendorId == tp.AttorneyInfo.VendorEntityId.Value);
                        if (vendor != null)
                        {
                            tpAttorneyVendorId = vendor.VendorId;
                            if (vendor.LegacyEntityId.HasValue && vendor.LegacyEntityId.Value > 0)
                                tpAttorneyEntityId = vendor.LegacyEntityId.Value;
                            else
                            {
                                var attorneyEntity = new Data.EntityMaster
                                {
                                    EntityType = 'B',
                                    PartyType = "Plaintiff Attorney",
                                    EntityGroupCode = "Vendor",
                                    EntityName = vendor.VendorName,
                                    ContactName = vendor.ContactName,
                                    HomeBusinessPhone = vendor.BusinessPhone ?? vendor.MobilePhone,
                                    Email = vendor.Email,
                                    EntityStatus = 'Y',
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = createdBy
                                };
                                _context.EntityMasters.Add(attorneyEntity);
                                await _context.SaveChangesAsync();

                                vendor.LegacyEntityId = attorneyEntity.EntityId;
                                _context.VendorMasters.Update(vendor);
                                await _context.SaveChangesAsync();

                                tpAttorneyEntityId = attorneyEntity.EntityId;
                            }
                        }
                    }
                    else
                    {
                        var newVendor = new Data.VendorMasterEntity
                        {
                            VendorName = tp.AttorneyInfo.FirmName ?? tp.AttorneyInfo.Name,
                            ContactName = tp.AttorneyInfo.Name,
                            BusinessPhone = tp.AttorneyInfo.PhoneNumber,
                            Email = tp.AttorneyInfo.Email,
                            VendorStatus = 'Y',
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        };
                        _context.VendorMasters.Add(newVendor);
                        await _context.SaveChangesAsync();

                        tpAttorneyVendorId = newVendor.VendorId;

                        var attorneyEntity = new Data.EntityMaster
                        {
                            EntityType = 'B',
                            PartyType = "Plaintiff Attorney",
                            EntityGroupCode = "Vendor",
                            EntityName = newVendor.VendorName,
                            ContactName = newVendor.ContactName,
                            HomeBusinessPhone = newVendor.BusinessPhone,
                            Email = newVendor.Email,
                            EntityStatus = 'Y',
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        };
                        _context.EntityMasters.Add(attorneyEntity);
                        await _context.SaveChangesAsync();

                        newVendor.LegacyEntityId = attorneyEntity.EntityId;
                        _context.VendorMasters.Update(newVendor);
                        await _context.SaveChangesAsync();

                        tpAttorneyEntityId = attorneyEntity.EntityId;
                    }
                }
                
                if (tp.Type == "Vehicle")
                {
                    long? ownerEntityId = null;
                    if (!string.IsNullOrEmpty(tp.Name))
                    {
                        var ownerEntity = new Data.EntityMaster 
                        { 
                            EntityType = 'I', 
                            PartyType = "ThirdPartyOwner", 
                            EntityGroupCode = "Claimant", 
                            EntityName = tp.Name, 
                            HomeBusinessPhone = tp.PhoneNumber,
                            Email = tp.Email,
                            EntityStatus = 'Y', 
                            CreatedDate = DateTime.Now, 
                            CreatedBy = createdBy 
                        };
                        _context.EntityMasters.Add(ownerEntity);
                        await _context.SaveChangesAsync();
                        ownerEntityId = ownerEntity.EntityId;
                        claimantEntityMap[tp.Name] = ownerEntityId.Value;
                        
                        // Save owner address if provided
                        if (tp.Address != null && tp.Address.HasAnyAddress)
                        {
                            _context.Addresses.Add(new Data.AddressMaster
                            {
                                EntityId = ownerEntity.EntityId,
                                AddressType = 'M',
                                StreetAddress = tp.Address.StreetAddress,
                                Apt = tp.Address.AddressLine2,
                                City = tp.Address.City,
                                State = tp.Address.State,
                                ZipCode = tp.Address.ZipCode,
                                CreatedDate = DateTime.Now,
                                CreatedBy = createdBy
                            });
                        }
                    }
                    
                    long? tpDriverEntityId = ownerEntityId;
                    if (tp.Driver != null && !string.IsNullOrEmpty(tp.Driver.Name) && !tp.IsDriverSameAsOwner)
                    {
                        var tpDriverEntity = new Data.EntityMaster 
                        { 
                            EntityType = 'I', 
                            PartyType = "ThirdPartyDriver", 
                            EntityGroupCode = "Claimant", 
                            EntityName = tp.Driver.Name, 
                            LicenseNumber = tp.Driver.LicenseNumber,
                            LicenseState = tp.Driver.LicenseState,
                            HomeBusinessPhone = tp.Driver.PhoneNumber,
                            Email = tp.Driver.Email,
                            DateOfBirth = tp.Driver.DateOfBirth != default ? tp.Driver.DateOfBirth : null,
                            EntityStatus = 'Y', 
                            CreatedDate = DateTime.Now, 
                            CreatedBy = createdBy 
                        };
                        _context.EntityMasters.Add(tpDriverEntity);
                        await _context.SaveChangesAsync();
                        tpDriverEntityId = tpDriverEntity.EntityId;
                        claimantEntityMap[tp.Driver.Name] = tpDriverEntityId.Value;
                        
                        // Save driver address if provided
                        if (tp.Driver.Address != null && tp.Driver.Address.HasAnyAddress)
                        {
                            _context.Addresses.Add(new Data.AddressMaster
                            {
                                EntityId = tpDriverEntity.EntityId,
                                AddressType = 'M',
                                StreetAddress = tp.Driver.Address.StreetAddress,
                                Apt = tp.Driver.Address.AddressLine2,
                                City = tp.Driver.Address.City,
                                State = tp.Driver.Address.State,
                                ZipCode = tp.Driver.Address.ZipCode,
                                CreatedDate = DateTime.Now,
                                CreatedBy = createdBy
                            });
                        }
                    }
                    
                    // Save third party vehicle
                    _context.Vehicles.Add(new Data.Vehicle 
                    { 
                        FnolId = fnol.FnolId, 
                        ClaimNumber = fnol.ClaimNumber, 
                        VehicleParty = "TPV", 
                        VIN = tp.Vehicle?.Vin, 
                        Make = tp.Vehicle?.Make, 
                        Model = tp.Vehicle?.Model, 
                        Year = tp.Vehicle?.Year ?? 0, 
                        PlateNumber = tp.Vehicle?.PlateNumber,
                        PlateState = tp.Vehicle?.PlateState,
                        IsVehicleDamaged = tp.Vehicle?.IsDamaged ?? false,
                        IsDrivable = tp.Vehicle?.IsDrivable ?? true,
                        InsuranceCarrier = tp.InsuranceCarrier,
                        InsurancePolicyNumber = tp.InsurancePolicyNumber,
                        VehicleOwnerEntityId = ownerEntityId, 
                        DriverEntityId = tpDriverEntityId, 
                        CreatedDate = DateTime.Now, 
                        CreatedBy = createdBy 
                    });
                    
                    // Create claimant record for injured party
                    if (tp.WasInjured)
                    {
                        var claimantEntityId = tp.InjuredParty == "Driver" ? tpDriverEntityId : ownerEntityId;
                        var claimantName = tp.InjuredParty == "Driver" ? (tp.Driver?.Name ?? tp.Name) : tp.Name;

                        // Use canonical 'Third Party' codes for new data (map legacy OVD/OVP -> TPD/TPP)
                        // Treat Owner and Driver as vehicle occupants (driver-type) — map both to TPD; passengers map to TPP
                        var claimantType = (tp.InjuredParty == "Driver" || tp.InjuredParty == "Owner") ? "TPD" : "TPP";

                        var tpClaimant = new Data.Claimant 
                        { 
                            FnolId = fnol.FnolId, 
                            ClaimNumber = fnol.ClaimNumber, 
                            ClaimantEntityId = claimantEntityId, 
                            ClaimantName = claimantName, 
                            ClaimantType = claimantType, 
                            InjuredParty = tp.InjuredParty, 
                            HasInjury = true,
                            IsAttorneyRepresented = (tpAttorneyVendorId.HasValue || tpAttorneyEntityId.HasValue),
                            AttorneyVendorId = tpAttorneyVendorId,
                            AttorneyEntityId = tpAttorneyEntityId,
                            CreatedDate = DateTime.Now, 
                            CreatedBy = createdBy 
                        };

                        // Add injury details
                        if (tp.InjuryInfo != null)
                        {
                            tpClaimant.InjuryType = tp.InjuryInfo.InjuryType;
                            tpClaimant.InjuryDescription = tp.InjuryInfo.InjuryDescription;
                            tpClaimant.InjurySeverity = tp.InjuryInfo.SeverityLevel.ToString();
                            tpClaimant.IsFatality = tp.InjuryInfo.IsFatality;
                            tpClaimant.IsHospitalized = tp.InjuryInfo.WasTakenToHospital;
                            tpClaimant.HospitalName = tp.InjuryInfo.HospitalName;
                            tpClaimant.HospitalStreetAddress = tp.InjuryInfo.HospitalStreetAddress;
                            tpClaimant.HospitalCity = tp.InjuryInfo.HospitalCity;
                            tpClaimant.HospitalState = tp.InjuryInfo.HospitalState;
                            tpClaimant.HospitalZipCode = tp.InjuryInfo.HospitalZipCode;
                            tpClaimant.TreatingPhysician = tp.InjuryInfo.TreatingPhysician;
                        }

                        _context.Claimants.Add(tpClaimant);
                    }
                }
                else
                {
                    // Non-vehicle third parties: Passenger, Pedestrian, Bicyclist, Other
                    var entity = new Data.EntityMaster 
                    { 
                        EntityType = 'I', 
                        PartyType = tp.Type, 
                        EntityGroupCode = "Claimant", 
                        EntityName = tp.Name, 
                        HomeBusinessPhone = tp.PhoneNumber, 
                        Email = tp.Email, 
                        EntityStatus = 'Y', 
                        CreatedDate = DateTime.Now, 
                        CreatedBy = createdBy 
                    };
                    _context.EntityMasters.Add(entity);
                    await _context.SaveChangesAsync();
                    claimantEntityMap[tp.Name] = entity.EntityId;
                    
                    // Save address if provided
                    if (tp.Address != null && tp.Address.HasAnyAddress)
                    {
                        _context.Addresses.Add(new Data.AddressMaster
                        {
                            EntityId = entity.EntityId,
                            AddressType = 'M',
                            StreetAddress = tp.Address.StreetAddress,
                            Apt = tp.Address.AddressLine2,
                            City = tp.Address.City,
                            State = tp.Address.State,
                            ZipCode = tp.Address.ZipCode,
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy
                        });
                    }
                    
                    var claimantType = tp.Type switch
                    {
                        "Passenger" => "TPP",
                        "Pedestrian" => "PED",
                        "Bicyclist" => "BYL",
                        _ => "OTH"
                    };
                    
                    var tpClaimant = new Data.Claimant 
                    { 
                        FnolId = fnol.FnolId, 
                        ClaimNumber = fnol.ClaimNumber, 
                        ClaimantEntityId = entity.EntityId, 
                        ClaimantName = tp.Name, 
                        ClaimantType = claimantType, 
                        HasInjury = tp.WasInjured,
                        IsAttorneyRepresented = (tpAttorneyVendorId.HasValue || tpAttorneyEntityId.HasValue),
                        AttorneyVendorId = tpAttorneyVendorId,
                        AttorneyEntityId = tpAttorneyEntityId,
                        CreatedDate = DateTime.Now, 
                        CreatedBy = createdBy 
                    };
                    
                    // Add injury details if injured
                    if (tp.WasInjured && tp.InjuryInfo != null)
                    {
                        tpClaimant.InjuryType = tp.InjuryInfo.InjuryType;
                        tpClaimant.InjuryDescription = tp.InjuryInfo.InjuryDescription;
                        tpClaimant.InjurySeverity = tp.InjuryInfo.SeverityLevel.ToString();
                        tpClaimant.IsFatality = tp.InjuryInfo.IsFatality;
                        tpClaimant.IsHospitalized = tp.InjuryInfo.WasTakenToHospital;
                        tpClaimant.HospitalName = tp.InjuryInfo.HospitalName;
                        tpClaimant.HospitalStreetAddress = tp.InjuryInfo.HospitalStreetAddress;
                        tpClaimant.HospitalCity = tp.InjuryInfo.HospitalCity;
                        tpClaimant.HospitalState = tp.InjuryInfo.HospitalState;
                        tpClaimant.HospitalZipCode = tp.InjuryInfo.HospitalZipCode;
                        tpClaimant.TreatingPhysician = tp.InjuryInfo.TreatingPhysician;
                    }
                    // If this is a passenger, persist the selected VIN reference on the claimant record
                    if (tp.Type == "Passenger")
                    {
                        tpClaimant.Vin = tp.SelectedVehicleVin;
                    }
                    
                    _context.Claimants.Add(tpClaimant);
                }
            }

            // Save property damages
            foreach (var pd in claim.PropertyDamages)
            {
                // Create entity for property owner
                var ownerEntity = new Data.EntityMaster
                {
                    EntityType = 'I',
                    PartyType = "PropertyOwner",
                    EntityGroupCode = "Claimant",
                    EntityName = pd.OwnerName,
                    HomeBusinessPhone = pd.OwnerPhoneNumber,
                    Email = pd.OwnerEmail,
                    EntityStatus = 'Y',
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdBy
                };
                _context.EntityMasters.Add(ownerEntity);
                await _context.SaveChangesAsync();
                
                // Save owner address
                if (pd.OwnerAddress != null && pd.OwnerAddress.HasAnyAddress)
                {
                    _context.Addresses.Add(new Data.AddressMaster
                    {
                        EntityId = ownerEntity.EntityId,
                        AddressType = 'M',
                        StreetAddress = pd.OwnerAddress.StreetAddress,
                        Apt = pd.OwnerAddress.AddressLine2,
                        City = pd.OwnerAddress.City,
                        State = pd.OwnerAddress.State,
                        ZipCode = pd.OwnerAddress.ZipCode,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy
                    });
                }

                _context.FnolPropertyDamages.Add(new Data.FnolPropertyDamage
                {
                    FnolId = fnol.FnolId,
                    ClaimNumber = fnol.ClaimNumber,
                    PropertyType = pd.PropertyType,
                    PropertyDescription = pd.Description,
                    DamageDescription = pd.DamageDescription,
                    EstimatedDamage = pd.EstimatedDamage,
                    OwnerEntityId = ownerEntity.EntityId,
                    // Denormalize owner and property address fields so reads don't rely solely on EntityMaster
                    OwnerName = pd.OwnerName,
                    OwnerPhone = pd.OwnerPhoneNumber,
                    OwnerEmail = pd.OwnerEmail,
                    OwnerAddress = pd.OwnerAddress?.StreetAddress,
                    OwnerAddress2 = pd.OwnerAddress?.AddressLine2,
                    OwnerCity = pd.OwnerAddress?.City,
                    OwnerState = pd.OwnerAddress?.State,
                    OwnerZipCode = pd.OwnerAddress?.ZipCode,
                    // Prefer explicit PropertyLocation; otherwise store the full formatted property address (street + city/state/zip)
                    PropertyLocation = string.IsNullOrWhiteSpace(pd.PropertyLocation) ? (pd.PropertyAddress != null ? pd.PropertyAddress.GetFormattedAddress() : string.Empty) : pd.PropertyLocation,
                    PropertyAddress = pd.PropertyAddress?.StreetAddress,
                    PropertyCity = pd.PropertyAddress?.City,
                    PropertyState = pd.PropertyAddress?.State,
                    PropertyZipCode = pd.PropertyAddress?.ZipCode,
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdBy
                });
            }

            // Save sub-claims
            int featureNumber = 1;
            // Determine base identifier for sub-claim numbers: prefer ClaimNumber, fall back to FnolNumber, then FnolId
            var baseClaimId = !string.IsNullOrEmpty(fnol.ClaimNumber)
                ? fnol.ClaimNumber
                : (!string.IsNullOrEmpty(fnol.FnolNumber) ? fnol.FnolNumber : $"F{fnol.FnolId}");
            foreach (var sc in claim.SubClaims)
            {
                var subClaim = new Data.SubClaim
                {
                    FnolId = fnol.FnolId,
                    ClaimNumber = baseClaimId,
                    SubClaimNumber = $"{baseClaimId}-{featureNumber:D2}",
                    FeatureNumber = featureNumber,
                    ClaimantName = sc.ClaimantName,
                    ClaimantType = sc.ClaimType,
                    Coverage = sc.Coverage,
                    CoverageLimits = sc.CoverageLimits,
                    ExpenseReserve = sc.ExpenseReserve,
                    IndemnityReserve = sc.IndemnityReserve,
                    AssignedAdjusterName = sc.AssignedAdjusterName,
                    SubClaimStatus = 'O',
                    OpenedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdBy
                };
                
                // Try to get claimant entity ID from map
                if (claimantEntityMap.TryGetValue(sc.ClaimantName, out long claimantEntityId))
                {
                    subClaim.ClaimantEntityId = claimantEntityId;
                }
                
                // Parse adjuster ID if provided
                if (!string.IsNullOrEmpty(sc.AssignedAdjusterId) && long.TryParse(sc.AssignedAdjusterId, out long adjusterId))
                {
                    subClaim.AssignedAdjusterId = adjusterId;
                }
                
                _context.SubClaims.Add(subClaim);
                featureNumber++;
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Search Operations

        public async Task<List<Data.Fnol>> SearchFnolsAsync(string? fnolNumber, string? policyNumber, DateTime? dateOfLossFrom, DateTime? dateOfLossTo, char? status)
        {
            var query = _context.FNOLs.AsQueryable();

            if (!string.IsNullOrEmpty(fnolNumber))
            {
                query = query.Where(f => f.FnolNumber == fnolNumber);
            }

            if (!string.IsNullOrEmpty(policyNumber))
            {
                query = query.Where(f => f.PolicyNumber == policyNumber);
            }

            if (dateOfLossFrom.HasValue)
            {
                query = query.Where(f => f.DateOfLoss >= dateOfLossFrom.Value);
            }

            if (dateOfLossTo.HasValue)
            {
                query = query.Where(f => f.DateOfLoss <= dateOfLossTo.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(f => f.FnolStatus == status.Value);
            }

            return await query.OrderByDescending(f => f.CreatedDate).ToListAsync();
        }

        public async Task<List<Data.Fnol>> SearchClaimsAsync(string? claimNumber, string? fnolNumber, string? policyNumber, DateTime? dateOfLossFrom, DateTime? dateOfLossTo, char? status)
        {
            var query = _context.FNOLs.AsQueryable();

            if (!string.IsNullOrEmpty(claimNumber))
            {
                query = query.Where(f => f.ClaimNumber == claimNumber);
            }

            if (!string.IsNullOrEmpty(fnolNumber))
            {
                query = query.Where(f => f.FnolNumber == fnolNumber);
            }

            if (!string.IsNullOrEmpty(policyNumber))
            {
                query = query.Where(f => f.PolicyNumber == policyNumber);
            }

            if (dateOfLossFrom.HasValue)
            {
                query = query.Where(f => f.DateOfLoss >= dateOfLossFrom.Value);
            }

            if (dateOfLossTo.HasValue)
            {
                query = query.Where(f => f.DateOfLoss <= dateOfLossTo.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(f => f.FnolStatus == status.Value);
            }

            return await query.OrderByDescending(f => f.CreatedDate).ToListAsync();
        }

        #endregion

        #region Claim Detail Operations

        public async Task<Claim?> GetClaimWithDetailsAsync(string claimNumber)
        {
            // Get the FNOL record associated with the claim number
            var fnol = await _context.FNOLs
                .FirstOrDefaultAsync(f => f.ClaimNumber == claimNumber || f.FnolNumber == claimNumber);

            if (fnol == null)
            {
                return null;
            }

            // Load related data separately
            var vehicles = await _context.Vehicles.Where(v => v.FnolId == fnol.FnolId).ToListAsync();
            var claimants = await _context.Claimants.Where(c => c.FnolId == fnol.FnolId).ToListAsync();
            var subClaims = await _context.SubClaims.Where(s => s.FnolId == fnol.FnolId).ToListAsync();
            var witnesses = await _context.FnolWitnesses.Where(w => w.FnolId == fnol.FnolId).ToListAsync();
            var authorities = await _context.FnolAuthorities.Where(a => a.FnolId == fnol.FnolId).ToListAsync();

            // Get entity details for witnesses and authorities
            var witnessEntityIds = witnesses.Select(w => w.EntityId).ToList();
            var authorityEntityIds = authorities.Select(a => a.EntityId).ToList();
            // Include addresses so we can display address details in the UI
            var witnessEntities = await _context.EntityMasters
                .Include(e => e.Addresses)
                .Where(e => witnessEntityIds.Contains(e.EntityId)).ToListAsync();
            var authorityEntities = await _context.EntityMasters
                .Include(e => e.Addresses)
                .Where(e => authorityEntityIds.Contains(e.EntityId)).ToListAsync();
            
            // Also load vendor records referenced by FnolAuthorities so we can show vendor addresses
            var vendorIds = authorities.Where(a => a.VendorId.HasValue).Select(a => a.VendorId!.Value).Distinct().ToList();
            var vendorMasters = new List<VendorMasterEntity>();
            if (vendorIds.Count > 0)
            {
                vendorMasters = await _context.VendorMasters
                    .Include(v => v.Addresses)
                    .Where(v => vendorIds.Contains(v.VendorId)).ToListAsync();
            }

            // Load reported-by entity/address if present so UI can show full address details
            Models.Address? reportedByAddress = null;
            if (fnol.ReportedByEntityId.HasValue)
            {
                var reportedEntity = await _context.EntityMasters
                    .Include(e => e.Addresses)
                    .FirstOrDefaultAsync(e => e.EntityId == fnol.ReportedByEntityId.Value);
                if (reportedEntity != null)
                {
                    var mainAddr = reportedEntity.Addresses.FirstOrDefault(a => a.AddressType == 'M');
                    if (mainAddr != null)
                    {
                        reportedByAddress = new Models.Address
                        {
                            StreetAddress = mainAddr.StreetAddress,
                            AddressLine2 = mainAddr.Apt,
                            City = mainAddr.City,
                            State = mainAddr.State,
                            ZipCode = mainAddr.ZipCode
                        };
                    }
                }
            }

            // Map status char to string
            var statusString = fnol.PolicyStatus switch
            {
                'Y' => "Active",
                'C' => "Cancelled",
                'E' => "Expired",
                _ => "Unknown"
            };

            // Map FNOL data to Claim model
            var claim = new Claim
            {
                ClaimNumber = fnol.ClaimNumber ?? string.Empty,
                PolicyNumber = fnol.PolicyNumber ?? string.Empty,
                Status = fnol.FnolStatus == 'O' ? "Open" : fnol.FnolStatus == 'C' ? "Closed" : "Draft",
                CreatedDate = fnol.CreatedDate,
                CreatedBy = fnol.CreatedBy ?? string.Empty,
                PolicyInfo = PolicyMapper.MapFnolToPolicy(fnol),
                LossDetails = new ClaimLossDetails
                {
                    DateOfLoss = fnol.DateOfLoss,
                    TimeOfLoss = fnol.TimeOfLoss.HasValue ? TimeOnly.FromTimeSpan(fnol.TimeOfLoss.Value) : TimeOnly.MinValue,
                    ReportDate = fnol.ReportDate,
                    ReportTime = fnol.ReportTime.HasValue ? TimeOnly.FromTimeSpan(fnol.ReportTime.Value) : TimeOnly.MinValue,
                    Location = fnol.LossLocation ?? string.Empty,
                    Location2 = fnol.LossLocation2,
                    CauseOfLoss = fnol.CauseOfLoss ?? string.Empty,
                    WeatherCondition = fnol.WeatherConditions ?? string.Empty,
                    LossDescription = fnol.LossDescription ?? string.Empty,
                    HasInjuries = fnol.HasInjury,
                    HasPropertyDamage = fnol.HasPropertyDamage,
                    HasOtherVehiclesInvolved = fnol.HasOtherVehiclesInvolved,
                    ReportedBy = fnol.ReportedBy ?? string.Empty,
                    ReportedByName = fnol.ReportedByName,
                    ReportedByPhone = fnol.ReportedByPhone,
                    ReportedByAddress = reportedByAddress,
                    ReportedByEmail = fnol.ReportedByEmail,
                    ReportingMethod = fnol.ReportingMethod ?? string.Empty,
                },
                InsuredParty = new InsuredPartyInfo
                {
                    Name = fnol.InsuredName ?? string.Empty,
                    PhoneNumber = fnol.InsuredPhone ?? string.Empty,
                    Email = fnol.InsuredEmail ?? string.Empty,
                    DoingBusinessAs = fnol.InsuredDoingBusinessAs ?? string.Empty,
                    // use the flat InsuredBusinessType field from the FNOL record
                    BusinessType = fnol.InsuredBusinessType ?? string.Empty,
                    Address = new Address
                    {
                        StreetAddress = fnol.InsuredAddress ?? string.Empty,
                        AddressLine2 = fnol.InsuredAddress2 ?? string.Empty,
                        City = fnol.InsuredCity ?? string.Empty,
                        State = fnol.InsuredState ?? string.Empty,
                        ZipCode = fnol.InsuredZipCode ?? string.Empty
                    },
                    FeinSsNumber = fnol.InsuredFeinSsNumber ?? string.Empty,
                    LicenseNumber = fnol.InsuredLicenseNumber ?? string.Empty,
                    LicenseState = fnol.InsuredLicenseState ?? string.Empty,
                    DateOfBirth = fnol.InsuredDateOfBirth ?? DateTime.MinValue
                },
                SubClaims = subClaims.Select(s => new Models.SubClaim
                {
                    Id = (int)s.SubClaimId,
                    FeatureNumber = s.FeatureNumber,
                    Coverage = s.Coverage ?? string.Empty,
                    CoverageLimits = s.CoverageLimits ?? string.Empty,
                    ClaimantName = s.ClaimantName ?? string.Empty,
                    ExpenseReserve = s.ExpenseReserve,
                    IndemnityReserve = s.IndemnityReserve,
                    AssignedAdjusterId = s.AssignedAdjusterId?.ToString() ?? string.Empty,
                    AssignedAdjusterName = s.AssignedAdjusterName ?? string.Empty,
                    Status = s.SubClaimStatus == 'O' ? "Open" : s.SubClaimStatus == 'C' ? "Closed" : "Unknown",
                    CreatedDate = s.CreatedDate,
                    ClaimType = s.ClaimantType ?? string.Empty,
                    IndemnityPaid = s.IndemnityPaid,
                    ExpensePaid = s.ExpensePaid,
                    Recoveries = s.Recoveries
                }).ToList()
            };

            // Populate Witnesses list by matching FnolWitness entries to EntityMaster records
            claim.LossDetails.Witnesses = witnesses.Select(w => {
                var entity = witnessEntities.FirstOrDefault(e => e.EntityId == w.EntityId);
                return new Witness
                {
                    Name = entity?.EntityName ?? string.Empty,
                    PhoneNumber = entity?.HomeBusinessPhone ?? string.Empty,
                    Email = entity?.Email ?? string.Empty,
                    Address = entity != null ? new Address
                    {
                        StreetAddress = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M')?.StreetAddress ?? entity.Addresses?.FirstOrDefault()?.StreetAddress ?? string.Empty,
                        AddressLine2 = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M')?.Apt ?? entity.Addresses?.FirstOrDefault()?.Apt ?? string.Empty,
                        City = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M')?.City ?? entity.Addresses?.FirstOrDefault()?.City ?? string.Empty,
                        State = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M')?.State ?? entity.Addresses?.FirstOrDefault()?.State ?? string.Empty,
                        ZipCode = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M')?.ZipCode ?? entity.Addresses?.FirstOrDefault()?.ZipCode ?? string.Empty
                    } : new Address()
                };
            }).ToList();

            // Populate AuthoritiesNotified list
            claim.LossDetails.AuthoritiesNotified = authorities.Select(a => {
                var vendor = a.VendorId.HasValue ? vendorMasters.FirstOrDefault(v => v.VendorId == a.VendorId.Value) : null;
                if (vendor != null)
                {
                    var mainAddr = vendor.Addresses?.FirstOrDefault(ad => ad.AddressType == 'M' && ad.AddressStatus == 'Y')
                                   ?? vendor.Addresses?.FirstOrDefault(ad => ad.AddressStatus == 'Y')
                                   ?? vendor.Addresses?.FirstOrDefault();

                    return new Authority
                    {
                        AuthorityName = a.AuthorityType ?? string.Empty,
                        Name = vendor.VendorName ?? string.Empty,
                        ReportNumber = a.ReportNumber,
                        Address = mainAddr != null ? new Address
                        {
                            StreetAddress = mainAddr.StreetAddress ?? string.Empty,
                            AddressLine2 = mainAddr.AddressLine2 ?? string.Empty,
                            City = mainAddr.City ?? string.Empty,
                            State = mainAddr.State ?? string.Empty,
                            ZipCode = mainAddr.ZipCode ?? string.Empty
                        } : new Address()
                    };
                }

                var entity = authorityEntities.FirstOrDefault(e => e.EntityId == a.EntityId);
                return new Authority
                {
                    AuthorityName = a.AuthorityType ?? string.Empty,
                    Name = entity?.EntityName ?? string.Empty,
                    ReportNumber = a.ReportNumber,
                    Address = entity != null ? new Address
                    {
                        StreetAddress = entity.Addresses?.FirstOrDefault(a2 => a2.AddressType == 'M')?.StreetAddress ?? entity.Addresses?.FirstOrDefault()?.StreetAddress ?? string.Empty,
                        AddressLine2 = entity.Addresses?.FirstOrDefault(a2 => a2.AddressType == 'M')?.Apt ?? entity.Addresses?.FirstOrDefault()?.Apt ?? string.Empty,
                        City = entity.Addresses?.FirstOrDefault(a2 => a2.AddressType == 'M')?.City ?? entity.Addresses?.FirstOrDefault()?.City ?? string.Empty,
                        State = entity.Addresses?.FirstOrDefault(a2 => a2.AddressType == 'M')?.State ?? entity.Addresses?.FirstOrDefault()?.State ?? string.Empty,
                        ZipCode = entity.Addresses?.FirstOrDefault(a2 => a2.AddressType == 'M')?.ZipCode ?? entity.Addresses?.FirstOrDefault()?.ZipCode ?? string.Empty
                    } : new Address()
                };
            }).ToList();

            // Load insured vehicle
            var insuredVehicle = vehicles.FirstOrDefault(v => v.VehicleParty == "IVD");
            if (insuredVehicle != null)
            {
                claim.InsuredVehicle = new VehicleInfo
                {
                    Vin = insuredVehicle.VIN ?? string.Empty,
                    Make = insuredVehicle.Make ?? string.Empty,
                    Model = insuredVehicle.Model ?? string.Empty,
                    Year = insuredVehicle.Year ?? 0,
                    PlateNumber = insuredVehicle.PlateNumber,
                    PlateState = insuredVehicle.PlateState,
                    IsDamaged = insuredVehicle.IsVehicleDamaged,
                    IsDrivable = insuredVehicle.IsDrivable,
                    WasTowed = insuredVehicle.WasTowed,
                    InStorage = insuredVehicle.IsInStorage,
                    StorageLocation = insuredVehicle.StorageLocation,
                    HasDashCam = insuredVehicle.HasDashCam,
                    DamageDetails = insuredVehicle.DamageDetails ?? string.Empty,
                    DidVehicleRollOver = insuredVehicle.DidVehicleRollOver,
                    HadWaterDamage = insuredVehicle.HadWaterDamage,
                    // map UI VehicleInfo properties to database Vehicle fields
                    HeadlightsWereOn = insuredVehicle.HeadlightsWereOn,
                    AirbagDeployed = insuredVehicle.AirbagDeployed,
                    DriverEntityId = insuredVehicle.DriverEntityId
                };

                // Load driver info if available
                if (insuredVehicle.DriverEntityId.HasValue)
                {
                    var driverEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == insuredVehicle.DriverEntityId.Value);
                    if (driverEntity != null)
                    {
                        var driverAddress = driverEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                                           ?? driverEntity.Addresses?.FirstOrDefault();
                        claim.Driver = new DriverInfo
                        {
                            Name = driverEntity.EntityName ?? string.Empty,
                            LicenseNumber = driverEntity.LicenseNumber ?? string.Empty,
                            LicenseState = driverEntity.LicenseState ?? string.Empty,
                            PhoneNumber = driverEntity.HomeBusinessPhone ?? string.Empty,
                            Email = driverEntity.Email ?? string.Empty,
                            DateOfBirth = driverEntity.DateOfBirth ?? default,
                            Address = driverAddress != null ? new Address
                            {
                                StreetAddress = driverAddress.StreetAddress ?? string.Empty,
                                AddressLine2 = driverAddress.Apt ?? string.Empty,
                                City = driverAddress.City ?? string.Empty,
                                State = driverAddress.State ?? string.Empty,
                                ZipCode = driverAddress.ZipCode ?? string.Empty
                            } : new Address()
                        };
                    }
                }
            }

            // Load driver claimant to get injury and attorney info
            var driverClaimant = claimants.FirstOrDefault(c => c.ClaimantType == "IVD");
            if (driverClaimant != null)
            {
                // Load driver injury
                if (driverClaimant.HasInjury)
                {
                    claim.DriverInjury = new Injury
                    {
                        InjuryType = driverClaimant.InjuryType,
                        InjuryDescription = driverClaimant.InjuryDescription,
                        SeverityLevel = driverClaimant.InjurySeverity != null ? int.Parse(driverClaimant.InjurySeverity) : 1,
                        IsFatality = driverClaimant.IsFatality,
                        WasTakenToHospital = driverClaimant.IsHospitalized,
                        HospitalName = driverClaimant.HospitalName,
                        HospitalStreetAddress = driverClaimant.HospitalStreetAddress,
                        HospitalCity = driverClaimant.HospitalCity,
                        HospitalState = driverClaimant.HospitalState,
                        HospitalZipCode = driverClaimant.HospitalZipCode,
                        TreatingPhysician = driverClaimant.TreatingPhysician
                    };
                }

                // Load driver attorney (prefer VendorMaster, fallback to legacy EntityMaster)
                if (driverClaimant.IsAttorneyRepresented)
                {
                    if (driverClaimant.AttorneyVendorId.HasValue)
                    {
                        var vendor = await _context.VendorMasters
                            .Include(v => v.Addresses)
                            .FirstOrDefaultAsync(v => v.VendorId == driverClaimant.AttorneyVendorId.Value);
                        if (vendor != null)
                        {
                            var mainAddr = vendor.Addresses?.FirstOrDefault(a => a.AddressType == 'M' && a.AddressStatus == 'Y')
                                           ?? vendor.Addresses?.FirstOrDefault(a => a.AddressStatus == 'Y')
                                           ?? vendor.Addresses?.FirstOrDefault();
                            claim.DriverAttorney = new AttorneyInfo
                            {
                                VendorEntityId = vendor.VendorId,
                                Name = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                FirmName = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                PhoneNumber = vendor.BusinessPhone ?? vendor.MobilePhone ?? string.Empty,
                                Email = vendor.Email ?? string.Empty,
                                Address = mainAddr != null ? new Address
                                {
                                    StreetAddress = mainAddr.StreetAddress ?? string.Empty,
                                    AddressLine2 = mainAddr.AddressLine2 ?? string.Empty,
                                    City = mainAddr.City ?? string.Empty,
                                    State = mainAddr.State ?? string.Empty,
                                    ZipCode = mainAddr.ZipCode ?? string.Empty
                                } : new Address()
                            };
                        }
                    }
                    else if (driverClaimant.AttorneyEntityId.HasValue)
                    {
                        var attorneyEntity = await _context.EntityMasters
                            .Include(e => e.Addresses)
                            .FirstOrDefaultAsync(e => e.EntityId == driverClaimant.AttorneyEntityId.Value);
                        if (attorneyEntity != null)
                        {
                            var attorneyAddress = attorneyEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                                                  ?? attorneyEntity.Addresses?.FirstOrDefault();
                            claim.DriverAttorney = new AttorneyInfo
                            {
                                VendorEntityId = attorneyEntity.EntityId,
                                Name = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                                FirmName = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                                PhoneNumber = attorneyEntity.HomeBusinessPhone ?? string.Empty,
                                Email = attorneyEntity.Email ?? string.Empty,
                                Address = attorneyAddress != null ? new Address
                                {
                                    StreetAddress = attorneyAddress.StreetAddress ?? string.Empty,
                                    AddressLine2 = attorneyAddress.Apt ?? string.Empty,
                                    City = attorneyAddress.City ?? string.Empty,
                                    State = attorneyAddress.State ?? string.Empty,
                                    ZipCode = attorneyAddress.ZipCode ?? string.Empty
                                } : new Address()
                            };
                        }
                    }
                }
            }

            // Load passengers (IVP claimants) with full attorney info
            var passengerClaimants = claimants.Where(c => c.ClaimantType == "IVP").ToList();
            foreach (var pc in passengerClaimants)
            {
                var passenger = new InsuredPassenger
                {
                    Name = pc.ClaimantName ?? string.Empty,
                    WasInjured = pc.HasInjury,
                    HasAttorney = pc.IsAttorneyRepresented
                };

                if (pc.ClaimantEntityId.HasValue)
                {
                    var entity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == pc.ClaimantEntityId.Value);
                    
                    if (entity != null)
                    {
                        passenger.PhoneNumber = entity.HomeBusinessPhone ?? string.Empty;
                        passenger.Email = entity.Email ?? string.Empty;
                        
                        var passengerAddress = entity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                                               ?? entity.Addresses?.FirstOrDefault();
                        if (passengerAddress != null)
                        {
                            passenger.Address = new Address
                            {
                                StreetAddress = passengerAddress.StreetAddress ?? string.Empty,
                                AddressLine2 = passengerAddress.Apt ?? string.Empty,
                                City = passengerAddress.City ?? string.Empty,
                                State = passengerAddress.State ?? string.Empty,
                                ZipCode = passengerAddress.ZipCode ?? string.Empty
                            };
                        }
                    }
                }

                if (pc.HasInjury)
                {
                    passenger.InjuryInfo = new Injury
                    {
                        InjuryType = pc.InjuryType,
                        InjuryDescription = pc.InjuryDescription,
                        SeverityLevel = pc.InjurySeverity != null ? int.Parse(pc.InjurySeverity) : 1,
                        IsFatality = pc.IsFatality,
                        WasTakenToHospital = pc.IsHospitalized,
                        HospitalName = pc.HospitalName,
                        HospitalStreetAddress = pc.HospitalStreetAddress,
                        HospitalCity = pc.HospitalCity,
                        HospitalState = pc.HospitalState,
                        HospitalZipCode = pc.HospitalZipCode,
                        TreatingPhysician = pc.TreatingPhysician
                    };
                }

                // Load passenger attorney (prefer VendorMaster)
                if (pc.IsAttorneyRepresented)
                {
                    if (pc.AttorneyVendorId.HasValue)
                    {
                        var vendor = await _context.VendorMasters
                            .Include(v => v.Addresses)
                            .FirstOrDefaultAsync(v => v.VendorId == pc.AttorneyVendorId.Value);
                        if (vendor != null)
                        {
                            var mainAddr = vendor.Addresses?.FirstOrDefault(a => a.AddressType == 'M' && a.AddressStatus == 'Y')
                                           ?? vendor.Addresses?.FirstOrDefault(a => a.AddressStatus == 'Y')
                                           ?? vendor.Addresses?.FirstOrDefault();
                            passenger.AttorneyInfo = new AttorneyInfo
                            {
                                VendorEntityId = vendor.VendorId,
                                Name = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                FirmName = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                PhoneNumber = vendor.BusinessPhone ?? vendor.MobilePhone ?? string.Empty,
                                Email = vendor.Email ?? string.Empty,
                                Address = mainAddr != null ? new Address
                                {
                                    StreetAddress = mainAddr.StreetAddress ?? string.Empty,
                                    AddressLine2 = mainAddr.AddressLine2 ?? string.Empty,
                                    City = mainAddr.City ?? string.Empty,
                                    State = mainAddr.State ?? string.Empty,
                                    ZipCode = mainAddr.ZipCode ?? string.Empty
                                } : new Address()
                            };
                        }
                    }
                    else if (pc.AttorneyEntityId.HasValue)
                    {
                        var attorneyEntity = await _context.EntityMasters
                            .Include(e => e.Addresses)
                            .FirstOrDefaultAsync(e => e.EntityId == pc.AttorneyEntityId.Value);
                        if (attorneyEntity != null)
                        {
                            var attorneyAddress = attorneyEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                                                  ?? attorneyEntity.Addresses?.FirstOrDefault();
                            passenger.AttorneyInfo = new AttorneyInfo
                            {
                                VendorEntityId = attorneyEntity.EntityId,
                                Name = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                                FirmName = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                                PhoneNumber = attorneyEntity.HomeBusinessPhone ?? string.Empty,
                                Email = attorneyEntity.Email ?? string.Empty,
                                Address = attorneyAddress != null ? new Address
                                {
                                    StreetAddress = attorneyAddress.StreetAddress ?? string.Empty,
                                    AddressLine2 = attorneyAddress.Apt ?? string.Empty,
                                    City = attorneyAddress.City ?? string.Empty,
                                    State = attorneyAddress.State ?? string.Empty,
                                    ZipCode = attorneyAddress.ZipCode ?? string.Empty
                                } : new Address()
                            };
                        }
                    }
                }

                claim.Passengers.Add(passenger);
            }

            // Load Vehicle Third Parties (TPV) from Vehicles table
            var vehicleThirdParties = vehicles.Where(v => v.VehicleParty == "TPV").ToList();
            foreach (var veh in vehicleThirdParties)
            {
                var party = new ThirdParty
                {
                    Type = "Vehicle",
                    Vehicle = new VehicleInfo
                    {
                        Vin = veh.VIN ?? string.Empty,
                        Make = veh.Make ?? string.Empty,
                        Model = veh.Model ?? string.Empty,
                        Year = veh.Year ?? 0,
                        PlateNumber = veh.PlateNumber,
                        PlateState = veh.PlateState,
                        IsDamaged = veh.IsVehicleDamaged,
                        IsDrivable = veh.IsDrivable
                    },
                    InsuranceCarrier = veh.InsuranceCarrier ?? string.Empty,
                    InsurancePolicyNumber = veh.InsurancePolicyNumber ?? string.Empty
                };

                // Load owner entity (if present)
                if (veh.VehicleOwnerEntityId.HasValue)
                {
                    var ownerEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == veh.VehicleOwnerEntityId.Value);
                    if (ownerEntity != null)
                    {
                        var ownerAddr = ownerEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') ?? ownerEntity.Addresses?.FirstOrDefault();
                        party.Name = ownerEntity.EntityName ?? string.Empty;
                        party.PhoneNumber = ownerEntity.HomeBusinessPhone ?? string.Empty;
                        party.Email = ownerEntity.Email ?? string.Empty;
                        if (ownerAddr != null)
                        {
                            party.Address = new Address
                            {
                                StreetAddress = ownerAddr.StreetAddress ?? string.Empty,
                                AddressLine2 = ownerAddr.Apt ?? string.Empty,
                                City = ownerAddr.City ?? string.Empty,
                                State = ownerAddr.State ?? string.Empty,
                                ZipCode = ownerAddr.ZipCode ?? string.Empty
                            };
                        }
                    }
                }

                // Load driver entity (if present)
                if (veh.DriverEntityId.HasValue)
                {
                    var driverEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == veh.DriverEntityId.Value);
                    if (driverEntity != null)
                    {
                        var driverAddr = driverEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') ?? driverEntity.Addresses?.FirstOrDefault();
                        party.Driver = new DriverInfo
                        {
                            Name = driverEntity.EntityName ?? string.Empty,
                            LicenseNumber = driverEntity.LicenseNumber ?? string.Empty,
                            LicenseState = driverEntity.LicenseState ?? string.Empty,
                            PhoneNumber = driverEntity.HomeBusinessPhone ?? string.Empty,
                            Email = driverEntity.Email ?? string.Empty,
                            DateOfBirth = driverEntity.DateOfBirth ?? default,
                            Address = driverAddr != null ? new Address
                            {
                                StreetAddress = driverAddr.StreetAddress ?? string.Empty,
                                AddressLine2 = driverAddr.Apt ?? string.Empty,
                                City = driverAddr.City ?? string.Empty,
                                State = driverAddr.State ?? string.Empty,
                                ZipCode = driverAddr.ZipCode ?? string.Empty
                            } : new Address()
                        };
                    }
                }

                // Try to find matching claimant(s) to populate injury and attorney info
                var vehicleClaimants = claimants.Where(c => c.ClaimantType != null && (c.ClaimantType.StartsWith("TP") || c.ClaimantType == "TPD" || c.ClaimantType == "TPP")).ToList();
                Data.Claimant? ownerClaimantVeh = null;
                Data.Claimant? driverClaimantVeh = null;

                if (veh.VehicleOwnerEntityId.HasValue)
                    ownerClaimantVeh = vehicleClaimants.FirstOrDefault(c => c.ClaimantEntityId.HasValue && c.ClaimantEntityId.Value == veh.VehicleOwnerEntityId.Value);
                if (veh.DriverEntityId.HasValue)
                    driverClaimantVeh = vehicleClaimants.FirstOrDefault(c => c.ClaimantEntityId.HasValue && c.ClaimantEntityId.Value == veh.DriverEntityId.Value);

                // If there is an injured claimant for this vehicle, map injury details
                var injuredClaimant = driverClaimantVeh ?? ownerClaimantVeh ?? vehicleClaimants.FirstOrDefault(c => c.HasInjury);
                if (injuredClaimant != null && injuredClaimant.HasInjury)
                {
                    party.WasInjured = true;
                    party.InjuryInfo = new Injury
                    {
                        InjuryType = injuredClaimant.InjuryType,
                        InjuryDescription = injuredClaimant.InjuryDescription,
                        SeverityLevel = injuredClaimant.InjurySeverity != null ? int.Parse(injuredClaimant.InjurySeverity) : 1,
                        IsFatality = injuredClaimant.IsFatality,
                        WasTakenToHospital = injuredClaimant.IsHospitalized,
                        HospitalName = injuredClaimant.HospitalName,
                        HospitalStreetAddress = injuredClaimant.HospitalStreetAddress,
                        HospitalCity = injuredClaimant.HospitalCity,
                        HospitalState = injuredClaimant.HospitalState,
                        HospitalZipCode = injuredClaimant.HospitalZipCode,
                        TreatingPhysician = injuredClaimant.TreatingPhysician
                    };
                    party.InjuredParty = injuredClaimant.InjuredParty;

                    // Map attorney info if present on claimant (prefer VendorMaster)
                    if (injuredClaimant.IsAttorneyRepresented)
                    {
                        if (injuredClaimant.AttorneyVendorId.HasValue)
                        {
                            var vendor = await _context.VendorMasters
                                .Include(v => v.Addresses)
                                .FirstOrDefaultAsync(v => v.VendorId == injuredClaimant.AttorneyVendorId.Value);
                            if (vendor != null)
                            {
                                var mainAddr = vendor.Addresses?.FirstOrDefault(a => a.AddressType == 'M' && a.AddressStatus == 'Y')
                                               ?? vendor.Addresses?.FirstOrDefault(a => a.AddressStatus == 'Y')
                                               ?? vendor.Addresses?.FirstOrDefault();
                                party.HasAttorney = true;
                                party.AttorneyInfo = new AttorneyInfo
                                {
                                    VendorEntityId = vendor.VendorId,
                                    Name = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                    FirmName = vendor.VendorName ?? vendor.ContactName ?? string.Empty,
                                    PhoneNumber = vendor.BusinessPhone ?? vendor.MobilePhone ?? string.Empty,
                                    Email = vendor.Email ?? string.Empty,
                                    Address = mainAddr != null ? new Address
                                    {
                                        StreetAddress = mainAddr.StreetAddress ?? string.Empty,
                                        AddressLine2 = mainAddr.AddressLine2 ?? string.Empty,
                                        City = mainAddr.City ?? string.Empty,
                                        State = mainAddr.State ?? string.Empty,
                                        ZipCode = mainAddr.ZipCode ?? string.Empty
                                    } : new Address()
                                };
                            }
                        }
                        else if (injuredClaimant.AttorneyEntityId.HasValue)
                        {
                            var attEntity = await _context.EntityMasters
                                .Include(e => e.Addresses)
                                .FirstOrDefaultAsync(e => e.EntityId == injuredClaimant.AttorneyEntityId.Value);
                            if (attEntity != null)
                            {
                                var attAddr = attEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') ?? attEntity.Addresses?.FirstOrDefault();
                                party.HasAttorney = true;
                                party.AttorneyInfo = new AttorneyInfo
                                {
                                    VendorEntityId = attEntity.EntityId,
                                    Name = attEntity.EntityName ?? attEntity.ContactName ?? string.Empty,
                                    FirmName = attEntity.EntityName ?? attEntity.ContactName ?? string.Empty,
                                    PhoneNumber = attEntity.HomeBusinessPhone ?? string.Empty,
                                    Email = attEntity.Email ?? string.Empty,
                                    Address = attAddr != null ? new Address
                                    {
                                        StreetAddress = attAddr.StreetAddress ?? string.Empty,
                                        AddressLine2 = attAddr.Apt ?? string.Empty,
                                        City = attAddr.City ?? string.Empty,
                                        State = attAddr.State ?? string.Empty,
                                        ZipCode = attAddr.ZipCode ?? string.Empty
                                    } : new Address()
                                };
                            }
                        }
                    }
                }

                claim.ThirdParties.Add(party);
            }

            // Load Non-Vehicle Third Parties (TPP, PED, BYL, BIC, OTH) from Claimants table
            var nonVehicleThirdPartyTypes = new[] { "TPP", "PED", "BYL", "BIC", "OTH", "OVP" };
            var nonVehicleClaimants = claimants.Where(c => nonVehicleThirdPartyTypes.Contains(c.ClaimantType ?? "")).ToList();

            // Diagnostic logging removed; keep nonVehicleClaimants available for mapping

            foreach (var tpClaimant in nonVehicleClaimants)
            {
                // Get entity details
                Data.EntityMaster? tpEntity = null;
                Data.AddressMaster? tpAddress = null;
                
                if (tpClaimant.ClaimantEntityId.HasValue)
                {
                    tpEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == tpClaimant.ClaimantEntityId.Value);
                    
                    tpAddress = tpEntity?.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                               ?? tpEntity?.Addresses?.FirstOrDefault();
                }

                // Map ClaimantType code back to Type string
                var thirdPartyType = tpClaimant.ClaimantType switch
                {
                    "TPP" => "Passenger",
                    "PED" => "Pedestrian",
                    "BYL" => "Bicyclist",
                    "BIC" => "Bicyclist",
                    _ => "Other"
                };

                var thirdParty = new ThirdParty
                {
                    Name = tpEntity?.EntityName ?? tpClaimant.ClaimantName ?? string.Empty,
                    Type = thirdPartyType,
                    PhoneNumber = tpEntity?.HomeBusinessPhone ?? string.Empty,
                    Email = tpEntity?.Email ?? string.Empty,
                    Address = tpAddress != null ? new Address
                    {
                        StreetAddress = tpAddress.StreetAddress ?? string.Empty,
                        AddressLine2 = tpAddress.Apt ?? string.Empty,
                        City = tpAddress.City ?? string.Empty,
                        State = tpAddress.State ?? string.Empty,
                        ZipCode = tpAddress.ZipCode ?? string.Empty
                    } : new Address(),
                    InjuredParty = tpClaimant.InjuredParty,
                    WasInjured = tpClaimant.HasInjury,
                    HasAttorney = tpClaimant.IsAttorneyRepresented
                };

                // If claimant record references a VIN (passenger assigned to a vehicle), map it back to the model
                if (!string.IsNullOrEmpty(tpClaimant.Vin))
                {
                    thirdParty.SelectedVehicleVin = tpClaimant.Vin;
                }

                if (tpClaimant.HasInjury)
                {
                    thirdParty.InjuryInfo = new Injury
                    {
                        InjuryType = tpClaimant.InjuryType,
                        InjuryDescription = tpClaimant.InjuryDescription,
                        SeverityLevel = tpClaimant.InjurySeverity != null ? int.Parse(tpClaimant.InjurySeverity) : 1,
                        IsFatality = tpClaimant.IsFatality,
                        WasTakenToHospital = tpClaimant.IsHospitalized,
                        HospitalName = tpClaimant.HospitalName,
                        HospitalStreetAddress = tpClaimant.HospitalStreetAddress,
                        HospitalCity = tpClaimant.HospitalCity,
                        HospitalState = tpClaimant.HospitalState,
                        HospitalZipCode = tpClaimant.HospitalZipCode,
                        TreatingPhysician = tpClaimant.TreatingPhysician
                    };
                }

                // Load attorney for vehicle third party
                if (tpClaimant.IsAttorneyRepresented && tpClaimant.AttorneyEntityId.HasValue)
                {
                    var attorneyEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == tpClaimant.AttorneyEntityId.Value);
                    if (attorneyEntity != null)
                    {
                        var attorneyAddress = attorneyEntity.Addresses?.FirstOrDefault(a => a.AddressType == 'M') 
                                              ?? attorneyEntity.Addresses?.FirstOrDefault();
                        thirdParty.AttorneyInfo = new AttorneyInfo
                        {
                            VendorEntityId = attorneyEntity.EntityId,
                            // Prefer firm/entity name as primary display
                            Name = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                            FirmName = attorneyEntity.EntityName ?? attorneyEntity.ContactName ?? string.Empty,
                            PhoneNumber = attorneyEntity.HomeBusinessPhone ?? string.Empty,
                            Email = attorneyEntity.Email ?? string.Empty,
                            Address = attorneyAddress != null ? new Address
                            {
                                StreetAddress = attorneyAddress.StreetAddress ?? string.Empty,
                                AddressLine2 = attorneyAddress.Apt ?? string.Empty,
                                City = attorneyAddress.City ?? string.Empty,
                                State = attorneyAddress.State ?? string.Empty,
                                ZipCode = attorneyAddress.ZipCode ?? string.Empty
                            } : new Address()
                        };
                    }
                }

                claim.ThirdParties.Add(thirdParty);
            }

            // Load property damages saved for this FNOL and map to Claim model
            var fnolPropertyDamages = await _context.FnolPropertyDamages
                .Where(pd => pd.FnolId == fnol.FnolId)
                .ToListAsync();

            foreach (var pd in fnolPropertyDamages)
            {
                // Try to resolve owner entity/address if OwnerEntityId present
                Data.EntityMaster? ownerEntity = null;
                Data.AddressMaster? ownerAddr = null;
                if (pd.OwnerEntityId.HasValue)
                {
                    ownerEntity = await _context.EntityMasters
                        .Include(e => e.Addresses)
                        .FirstOrDefaultAsync(e => e.EntityId == pd.OwnerEntityId.Value);
                    ownerAddr = ownerEntity?.Addresses?.FirstOrDefault(a => a.AddressType == 'M')
                                ?? ownerEntity?.Addresses?.FirstOrDefault();
                }

                var ownerAddressModel = new Address
                {
                    StreetAddress = pd.OwnerAddress ?? ownerAddr?.StreetAddress ?? string.Empty,
                    AddressLine2 = pd.OwnerAddress2 ?? ownerAddr?.Apt ?? string.Empty,
                    City = pd.OwnerCity ?? ownerAddr?.City ?? string.Empty,
                    State = pd.OwnerState ?? ownerAddr?.State ?? string.Empty,
                    ZipCode = pd.OwnerZipCode ?? ownerAddr?.ZipCode ?? string.Empty
                };

                var propertyAddressModel = new Address
                {
                    StreetAddress = pd.PropertyAddress ?? string.Empty,
                    AddressLine2 = string.Empty,
                    City = pd.PropertyCity ?? string.Empty,
                    State = pd.PropertyState ?? string.Empty,
                    ZipCode = pd.PropertyZipCode ?? string.Empty
                };

                claim.PropertyDamages.Add(new PropertyDamage
                {
                    Id = (int)pd.FnolPropertyDamageId,
                    PropertyType = pd.PropertyType ?? string.Empty,
                    Description = pd.PropertyDescription ?? string.Empty,
                    OwnerName = pd.OwnerName ?? ownerEntity?.EntityName ?? string.Empty,
                    OwnerAddress = ownerAddressModel,
                    OwnerPhoneNumber = pd.OwnerPhone ?? ownerEntity?.HomeBusinessPhone ?? string.Empty,
                    OwnerEmail = pd.OwnerEmail ?? ownerEntity?.Email ?? string.Empty,
                    OwnerFeinSsNumber = string.Empty,
                    PropertyLocation = pd.PropertyLocation ?? string.Empty,
                    PropertyAddress = propertyAddressModel,
                    DamageDescription = pd.DamageDescription ?? string.Empty,
                    EstimatedDamage = pd.EstimatedDamage ?? 0m
                });
            }

            // Diagnostic: log how many property damages were mapped for this claim
            try
            {
                Console.WriteLine($"[DEBUG] Loaded claim {claim.ClaimNumber} with PropertyDamages.Count={claim.PropertyDamages.Count}");
            }
            catch
            {
                // Ignore logging errors in diagnostics
            }

            return claim;
        }

        #endregion

        #region Number Generation

        public async Task<string> GenerateFnolNumberAsync()
        {
            var yearPrefix = DateTime.Now.ToString("yy");
            var lastFnol = await _context.FNOLs
                .Where(f => f.FnolNumber != null && f.FnolNumber.StartsWith("F" + yearPrefix))
                .OrderByDescending(f => f.FnolNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastFnol?.FnolNumber != null && lastFnol.FnolNumber.Length > 3)
            {
                var seqPart = lastFnol.FnolNumber.Substring(3);
                if (int.TryParse(seqPart, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"F{yearPrefix}{sequence:D6}";
        }

        public async Task<string> GenerateClaimNumberAsync()
        {
            var yearPrefix = DateTime.Now.ToString("yy");
            var lastClaim = await _context.FNOLs
                .Where(f => f.ClaimNumber != null && f.ClaimNumber.StartsWith("C" + yearPrefix))
                .OrderByDescending(f => f.ClaimNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastClaim?.ClaimNumber != null && lastClaim.ClaimNumber.Length > 3)
            {
                var seqPart = lastClaim.ClaimNumber.Substring(3);
                if (int.TryParse(seqPart, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"C{yearPrefix}{sequence:D6}";
        }

        #endregion

        #region Sub-Claim Operations

        public async Task<Data.SubClaim> CreateSubClaimAsync(Data.SubClaim subClaim)
        {
            subClaim.CreatedDate = DateTime.Now;
            subClaim.OpenedDate = DateTime.Now;
            subClaim.SubClaimStatus = 'O'; // Open
            
            _context.SubClaims.Add(subClaim);
            await _context.SaveChangesAsync();
            return subClaim;
        }

        public async Task<List<Data.SubClaim>> GetSubClaimsAsync(long fnolId)
        {
            return await _context.SubClaims
                .Where(s => s.FnolId == fnolId)
                .OrderBy(s => s.FeatureNumber)
                .ToListAsync();
        }

        public async Task<Data.SubClaim> UpdateSubClaimAsync(Data.SubClaim subClaim)
        {
            subClaim.ModifiedDate = DateTime.Now;
            _context.SubClaims.Update(subClaim);
            await _context.SaveChangesAsync();
            return subClaim;
        }

        public async Task CloseSubClaimWithDetailsAsync(int subClaimId, string reason, string remarks, string closedBy)
        {
            var subClaim = await _context.SubClaims.FindAsync((long)subClaimId);
            if (subClaim != null)
            {
                subClaim.SubClaimStatus = 'C'; // Closed
                subClaim.ClosedDate = DateTime.Now;
                subClaim.ClosedBy = closedBy;
                subClaim.CloseReason = string.IsNullOrEmpty(remarks) ? reason : $"{reason}: {remarks}";
                subClaim.ModifiedDate = DateTime.Now;
                subClaim.ModifiedBy = closedBy;

                await _context.SaveChangesAsync();
            }
        }

        public async Task ReopenSubClaimWithDetailsAsync(int subClaimId, string reason, string remarks, decimal expenseReserve, decimal indemnityReserve, string reopenedBy)
        {
            var subClaim = await _context.SubClaims.FindAsync((long)subClaimId);
            if (subClaim != null)
            {
                subClaim.SubClaimStatus = 'O'; // Open
                subClaim.ReopenedDate = DateTime.Now;
                subClaim.ReopenedBy = reopenedBy;
                subClaim.ReopenReason = string.IsNullOrEmpty(remarks) ? reason : $"{reason}: {remarks}";
                subClaim.ExpenseReserve = expenseReserve;
                subClaim.IndemnityReserve = indemnityReserve;
                subClaim.ClosedDate = null;
                subClaim.ClosedBy = null;
                subClaim.CloseReason = null;
                subClaim.ModifiedDate = DateTime.Now;
                subClaim.ModifiedBy = reopenedBy;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<PreviousReservesResult> GetPreviousReservesAsync(int subClaimId)
        {
            var subClaim = await _context.SubClaims.FindAsync((long)subClaimId);
            return new PreviousReservesResult
            {
                ExpenseReserve = subClaim?.ExpenseReserve ?? 0,
                IndemnityReserve = subClaim?.IndemnityReserve ?? 0
            };
        }

        #endregion

        #region Vehicle Operations

        public async Task<Data.Vehicle> AddVehicleAsync(Data.Vehicle vehicle)
        {
            vehicle.CreatedDate = DateTime.Now;
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<List<Data.Vehicle>> GetVehiclesAsync(long fnolId)
        {
            return await _context.Vehicles
                .Where(v => v.FnolId == fnolId)
                .ToListAsync();
        }

        #endregion

        #region Claimant Operations

        public async Task<Data.Claimant> AddClaimantAsync(Data.Claimant claimant)
        {
            claimant.CreatedDate = DateTime.Now;
            _context.Claimants.Add(claimant);
            await _context.SaveChangesAsync();
            return claimant;
        }

        public async Task<List<Data.Claimant>> GetClaimantsAsync(long fnolId)
        {
            return await _context.Claimants
                .Where(c => c.FnolId == fnolId)
                .ToListAsync();
        }

        #endregion
    }
}
