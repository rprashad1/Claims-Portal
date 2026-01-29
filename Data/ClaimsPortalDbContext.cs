using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Models;

namespace ClaimsPortal.Data
{
    public class ClaimsPortalDbContext : DbContext
    {
        public ClaimsPortalDbContext(DbContextOptions<ClaimsPortalDbContext> options)
            : base(options)
        {
        }

        // DbSets for all tables
        public DbSet<LookupCode> LookupCodes { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Fnol> FNOLs { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<EntityMaster> EntityMasters { get; set; }
        public DbSet<AddressMaster> Addresses { get; set; }
        public DbSet<SubClaim> SubClaims { get; set; }
        public DbSet<Claimant> Claimants { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<FnolWitness> FnolWitnesses { get; set; }
        public DbSet<FnolAuthority> FnolAuthorities { get; set; }
        public DbSet<SubClaimAudit> SubClaimAudits { get; set; }
        public DbSet<FnolPropertyDamage> FnolPropertyDamages { get; set; }

        // Vendor Master Tables (separate from EntityMaster for performance)
        public DbSet<VendorMasterEntity> VendorMasters { get; set; }
        public DbSet<VendorAddressEntity> VendorAddresses { get; set; }
        
        // Letter generation admin tables
        public DbSet<LetterGenAdminRule> LetterGenAdminRules { get; set; }
        public DbSet<LetterGenGeneratedDocument> LetterGenGeneratedDocuments { get; set; }
        public DbSet<LetterGenQueue> LetterGenQueue { get; set; }
        public DbSet<LetterGenFormData> LetterGenFormData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match database
            modelBuilder.Entity<LookupCode>().ToTable("LookupCodes");
            modelBuilder.Entity<Policy>().ToTable("Policies");
            modelBuilder.Entity<Fnol>().ToTable("FNOL");
            modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
            modelBuilder.Entity<EntityMaster>().ToTable("EntityMaster");
            modelBuilder.Entity<AddressMaster>().ToTable("AddressMaster");
            modelBuilder.Entity<SubClaim>().ToTable("SubClaims");
            modelBuilder.Entity<Claimant>().ToTable("Claimants");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLog");
            modelBuilder.Entity<FnolWitness>().ToTable("FnolWitnesses");
            modelBuilder.Entity<FnolAuthority>().ToTable("FnolAuthorities");
            modelBuilder.Entity<SubClaimAudit>().ToTable("SubClaimAudits");
            modelBuilder.Entity<FnolPropertyDamage>().ToTable("FnolPropertyDamages");

            // Vendor Master tables
            modelBuilder.Entity<VendorMasterEntity>().ToTable("VendorMaster");
            modelBuilder.Entity<VendorAddressEntity>().ToTable("VendorAddress");

            // Letter generation admin tables (map to existing DB names)
            modelBuilder.Entity<LetterGenAdminRule>().ToTable("LetterGen_AdminRules");
            modelBuilder.Entity<LetterGenGeneratedDocument>().ToTable("LetterGen_GeneratedDocuments");
            modelBuilder.Entity<LetterGenQueue>().ToTable("LetterGen_Queue");
                modelBuilder.Entity<LetterGenFormData>().ToTable("LetterGen_FormData");

            // Configure key mappings
            modelBuilder.Entity<LookupCode>().HasKey(l => l.LookupCodeId);
            modelBuilder.Entity<Policy>().HasKey(p => p.PolicyId);
            modelBuilder.Entity<Fnol>().HasKey(f => f.FnolId);
            modelBuilder.Entity<Vehicle>().HasKey(v => v.VehicleId);
            modelBuilder.Entity<EntityMaster>().HasKey(e => e.EntityId);
            modelBuilder.Entity<AddressMaster>().HasKey(a => a.AddressId);
            modelBuilder.Entity<SubClaim>().HasKey(s => s.SubClaimId);
            modelBuilder.Entity<Claimant>().HasKey(c => c.ClaimantId);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.AuditLogId);
            modelBuilder.Entity<FnolWitness>().HasKey(fw => fw.FnolWitnessId);
            modelBuilder.Entity<FnolAuthority>().HasKey(fa => fa.FnolAuthorityId);
            modelBuilder.Entity<SubClaimAudit>().HasKey(sca => sca.SubClaimAuditId);
            modelBuilder.Entity<FnolPropertyDamage>().HasKey(fpd => fpd.FnolPropertyDamageId);

            // Vendor Master keys
            modelBuilder.Entity<VendorMasterEntity>().HasKey(v => v.VendorId);
            modelBuilder.Entity<VendorAddressEntity>().HasKey(va => va.VendorAddressId);

            // Letter generation admin keys
            modelBuilder.Entity<LetterGenAdminRule>().HasKey(l => l.Id);
            modelBuilder.Entity<LetterGenGeneratedDocument>().HasKey(d => d.Id);
            modelBuilder.Entity<LetterGenQueue>().HasKey(q => q.QueueId);
            modelBuilder.Entity<LetterGenFormData>().HasKey(f => f.Id);

            // Configure relationships
            // NOTE: FNOL -> Policy relationship removed because Policy comes from external system
            // The PolicyNumber in FNOL is just a reference string, not a foreign key

            modelBuilder.Entity<Vehicle>()
                .HasOne<Fnol>()
                .WithMany()
                .HasForeignKey(v => v.FnolId);

            modelBuilder.Entity<SubClaim>()
                .HasOne<Fnol>()
                .WithMany()
                .HasForeignKey(s => s.FnolId);

            modelBuilder.Entity<Claimant>()
                .HasOne<Fnol>()
                .WithMany()
                .HasForeignKey(c => c.FnolId);

            modelBuilder.Entity<AddressMaster>()
                .HasOne<EntityMaster>()
                .WithMany(e => e.Addresses)
                .HasForeignKey(a => a.EntityId);

            // Vendor Address relationship
            modelBuilder.Entity<VendorAddressEntity>()
                .HasOne<VendorMasterEntity>()
                .WithMany(v => v.Addresses)
                .HasForeignKey(a => a.VendorId);

            // Configure decimal precision for money fields in SubClaims
            modelBuilder.Entity<SubClaim>()
                .Property(s => s.IndemnityPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubClaim>()
                .Property(s => s.IndemnityReserve)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubClaim>()
                .Property(s => s.ExpensePaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubClaim>()
                .Property(s => s.ExpenseReserve)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubClaim>()
                .Property(s => s.Reimbursement)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubClaim>()
                .Property(s => s.Recoveries)
                .HasPrecision(18, 2);
        }
    }

    // Entity models matching database structure
    public class LookupCode
    {
        public long LookupCodeId { get; set; }
        public string? RecordType { get; set; }
        public string? RecordCode { get; set; }
        public string? RecordDescription { get; set; }
        public char RecordStatus { get; set; } = 'Y';
        public int? SortOrder { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    // Letter generation admin rule entity
    public class LetterGenAdminRule
    {
        public long Id { get; set; }
        public string Coverage { get; set; } = string.Empty;         // e.g. BI
        public string Claimant { get; set; } = string.Empty;         // e.g. Insured Vehicle Driver
        public bool IsAttorneyRepresented { get; set; }              // true = represented
        public string DocumentName { get; set; } = string.Empty;     // e.g. BI Sign Letter
        public string? TemplateFile { get; set; }
        public string MailTo { get; set; } = string.Empty;           // e.g. Claimant;Attorney
        public string Location { get; set; } = string.Empty;         // UNC or path
        public int Priority { get; set; } = 100;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public string? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    // Generated document metadata
    public class LetterGenGeneratedDocument
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public long? RuleId { get; set; }
        public string? ClaimNumber { get; set; }
        public long? SubClaimId { get; set; }
        public int? SubClaimFeatureNumber { get; set; }
        public string? DocumentNumber { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = "filesystem"; // filesystem, azure, s3, db
        public string? StoragePath { get; set; }
        public byte[]? Content { get; set; }
        public string ContentType { get; set; } = "application/pdf";
        public long? FileSize { get; set; }
        public string? Sha256Hash { get; set; }
        public string? MailTo { get; set; }
        public string? MailStatus { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    }

    // Durable queue for letter generation jobs
    public class LetterGenQueue
    {
        public long QueueId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        // Comma-separated rule ids (nullable = generate all)
        public string? SelectedRuleIds { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
        public int Tries { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastAttemptAt { get; set; }
        public string? LastError { get; set; }
        public string? ProcessingHostname { get; set; }
    }

    public class Policy
    {
        public long PolicyId { get; set; }
        public string? PolicyNumber { get; set; }
        public long? InsuredEntityId { get; set; }
        public string? PolicyType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public char PolicyStatus { get; set; } = 'Y';
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class Fnol
    {
        public long FnolId { get; set; }
        public string? FnolNumber { get; set; }
        public string? ClaimNumber { get; set; }
        public string? PolicyNumber { get; set; }
        public DateTime? PolicyEffectiveDate { get; set; }
        public DateTime? PolicyExpirationDate { get; set; }
        public DateTime? PolicyCancelDate { get; set; }
        public char? PolicyStatus { get; set; }

        // Insured Party Details (complete information)
        public string? InsuredName { get; set; }
        public string? InsuredPhone { get; set; }
        public string? InsuredEmail { get; set; }
        public string? InsuredDoingBusinessAs { get; set; }
        public string? InsuredBusinessType { get; set; }
        public string? InsuredAddress { get; set; }
        public string? InsuredAddress2 { get; set; }
        public string? InsuredCity { get; set; }
        public string? InsuredState { get; set; }
        public string? InsuredZipCode { get; set; }
        public string? InsuredFeinSsNumber { get; set; }
        public string? InsuredLicenseNumber { get; set; }
        public string? InsuredLicenseState { get; set; }
        public DateTime? InsuredDateOfBirth { get; set; }
        public int? RenewalNumber { get; set; }

        public DateTime DateOfLoss { get; set; }
        public TimeSpan? TimeOfLoss { get; set; }
        public DateTime ReportDate { get; set; }
        public TimeSpan? ReportTime { get; set; }
        public string? LossLocation { get; set; }
        public string? LossLocation2 { get; set; }              // Loss Location Line 2
        public string? CauseOfLoss { get; set; }
        public string? WeatherConditions { get; set; }
        public string? LossDescription { get; set; }
        public bool HasVehicleDamage { get; set; }
        public bool HasInjury { get; set; }
        public bool HasPropertyDamage { get; set; }
        public bool HasOtherVehiclesInvolved { get; set; }

        // Reported By Information
        public string? ReportedBy { get; set; }              // Insured, Agent, Other
        public string? ReportedByName { get; set; }          // Name if Other
        public string? ReportedByPhone { get; set; }         // Phone if Other
        public string? ReportedByEmail { get; set; }         // Email if Other
        public string? ReportingMethod { get; set; }         // Phone, Email, Web, Fax
        public long? ReportedByEntityId { get; set; }       // FK to EntityMaster if Other

        public char FnolStatus { get; set; } = 'O';
        public DateTime? ClaimCreatedDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public TimeSpan CreatedTime { get; set; } = DateTime.Now.TimeOfDay;  // Required - non-nullable with default
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public TimeSpan? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class Vehicle
    {
        public long VehicleId { get; set; }
        public long FnolId { get; set; }
        public string? ClaimNumber { get; set; }
        public string? VehicleParty { get; set; }
        public bool VehicleListed { get; set; }
        public string? VIN { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? PlateNumber { get; set; }
        public string? PlateState { get; set; }
        public bool IsVehicleDamaged { get; set; }
        public bool WasTowed { get; set; }
        public bool IsInStorage { get; set; }
        public bool IsDrivable { get; set; }
        public bool HeadlightsWereOn { get; set; }
        public bool HadWaterDamage { get; set; }
        public bool AirbagDeployed { get; set; }
        public bool HasDashCam { get; set; }           // Dash Cam Installed
        public bool DidVehicleRollOver { get; set; }   // Vehicle Did Roll Over
        public string? StorageLocation { get; set; }
        public string? DamageDetails { get; set; }

        // Entity references for vehicle owner and driver
        public long? VehicleOwnerEntityId { get; set; }  // FK to EntityMaster - Vehicle Owner
        public long? DriverEntityId { get; set; }        // FK to EntityMaster - Driver (if different from owner)

        // Third Party Insurance Information (for TPV - Third Party Vehicles)
        public string? InsuranceCarrier { get; set; }         // Third party's insurance carrier
        public string? InsurancePolicyNumber { get; set; }    // Third party's policy number

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class EntityMaster
    {
        public long EntityId { get; set; }
        public char EntityType { get; set; }                    // 'B' = Business, 'I' = Individual
        public string? PartyType { get; set; }                   // Vendor Type (Medical Providers, Hospitals, etc.)
        public string? EntityGroupCode { get; set; }             // Default to 'Vendor' for Vendor Master
        public string? VendorType { get; set; }                  // Same as PartyType for vendors
        public string? EntityName { get; set; }                  // Business Name
        public string? DBA { get; set; }                         // Doing Business As
        public DateTime? EntityEffectiveDate { get; set; }      // Effective Date
        public DateTime? EntityTerminationDate { get; set; }    // Termination Date
        public DateTime? DateOfBirth { get; set; }
        public string? FEINorSS { get; set; }                    // FEIN #
        public string? LicenseNumber { get; set; }
        public string? LicenseState { get; set; }
        public string? ContactName { get; set; }                 // NEW: Contact Name
        public string? HomeBusinessPhone { get; set; }           // Business Phone #
        public string? MobilePhone { get; set; }                 // Mobile Phone #
        public string? FaxNumber { get; set; }                   // NEW: Fax #
        public string? Email { get; set; }                       // Email ID
        public bool W9Received { get; set; }                    // RENAMED from Is1099Reportable
        public bool IsSubjectTo1099 { get; set; }               // Subject to 1099
        public bool IsBackupWithholding { get; set; }           // Backup Withholding
        public bool ReceivesBulkPayment { get; set; }           // Receives Bulk Payments
        public string? PaymentFrequency { get; set; }            // Bulk Payment Frequency (Monthly/Weekly)
        public string? BulkPaymentDayDate1 { get; set; }         // Payment dates/days (comma-separated)
        public string? BulkPaymentDayDate2 { get; set; }         // Additional payment dates/days
        public char EntityStatus { get; set; } = 'Y';           // Status: 'Y' = Active, 'D' = Disabled
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; } // NEW: ModifiedBy property

        // Navigation property for addresses
        public virtual ICollection<AddressMaster> Addresses { get; set; } = new List<AddressMaster>();
    }

    public class AddressMaster
    {
        public long AddressId { get; set; }
        public long EntityId { get; set; }
        public char AddressType { get; set; }                   // 'M' = Main, 'A' = Alternate, 'T' = Temporary
        public string? StreetAddress { get; set; }               // Address 1
        public string? Apt { get; set; }                         // Address 2
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; } = "USA";
        public string? ZipCode { get; set; }                     // ZIP
        public string? HomeBusinessPhone { get; set; }
        public string? MobilePhone { get; set; }
        public string? FaxNumber { get; set; }                   // NEW: Fax #
        public string? Email { get; set; }
        public char AddressStatus { get; set; } = 'Y';          // Status: 'Y' = Active, 'D' = Disabled
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class SubClaim
    {
        public long SubClaimId { get; set; }
        public long FnolId { get; set; }
        public string? ClaimNumber { get; set; }
        public string? SubClaimNumber { get; set; }
        public int FeatureNumber { get; set; }
        public string? ClaimantName { get; set; }
        public string? ClaimantType { get; set; }
        public long? ClaimantEntityId { get; set; }          // FK to EntityMaster for direct entity linking
        public string? Coverage { get; set; }
        public string? CoverageLimits { get; set; }          // Changed from decimal to string to support "100,000/300,000" format
        public long? AssignedAdjusterId { get; set; }
        public string? AssignedAdjusterName { get; set; }    // Denormalized adjuster name for display
        public char SubClaimStatus { get; set; } = 'O';
        public DateTime? OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string? ClosedBy { get; set; }                // Who closed the sub-claim
        public string? CloseReason { get; set; }             // Reason for closing
        public DateTime? ReopenedDate { get; set; }          // When reopened (if applicable)
        public string? ReopenedBy { get; set; }              // Who reopened the sub-claim
        public string? ReopenReason { get; set; }            // Reason for reopening
        public decimal IndemnityPaid { get; set; }
        public decimal IndemnityReserve { get; set; }
        public decimal ExpensePaid { get; set; }
        public decimal ExpenseReserve { get; set; }
        public decimal Reimbursement { get; set; }
        public decimal Recoveries { get; set; }
        public string? SubrogationFileNumber { get; set; }
        public long? SubrogationAdjusterId { get; set; }
        public string? ArbitrationFileNumber { get; set; }
        public long? ArbitrationAdjusterId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class Claimant
    {
        public long ClaimantId { get; set; }
        public long FnolId { get; set; }
        public string? ClaimNumber { get; set; }
        public string? ClaimantName { get; set; }
        public long? ClaimantEntityId { get; set; }
        public string? ClaimantType { get; set; }

        /// <summary>
        /// For third-party vehicle claims: Identifies who is the injured party and claimant.
        /// Values: "Owner" or "Driver"
        /// When driver is different from owner, this field indicates which person is the claimant.
        /// </summary>
        public string? InjuredParty { get; set; }

        public bool HasInjury { get; set; }
        public string? InjuryType { get; set; }
        public string? InjurySeverity { get; set; }
        public string? InjuryDescription { get; set; }
        public bool IsFatality { get; set; }
        public bool IsHospitalized { get; set; }

        // Hospital Information (when IsHospitalized = true)
        public string? HospitalName { get; set; }
        public string? HospitalStreetAddress { get; set; }
        public string? HospitalCity { get; set; }
        public string? HospitalState { get; set; }
        public string? HospitalZipCode { get; set; }
        public string? TreatingPhysician { get; set; }

        // Vendor references (for VendorMaster integration)
        public long? HospitalVendorId { get; set; }      // FK to VendorMaster
        public long? AttorneyVendorId { get; set; }      // FK to VendorMaster

        public bool IsAttorneyRepresented { get; set; }
        public long? AttorneyEntityId { get; set; }      // Legacy FK to EntityMaster
        // VIN reference for claimant (used for passenger third-parties to reference which vehicle they were in)
        public string? Vin { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public string? TableName { get; set; }
        public long RecordId { get; set; }
        public string? TransactionType { get; set; }
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? UserId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? IPAddress { get; set; }
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Links witnesses to FNOL records
    /// </summary>
    public class FnolWitness
    {
        public long FnolWitnessId { get; set; }
        public long FnolId { get; set; }
        public long EntityId { get; set; }                  // FK to EntityMaster
        public string? WitnessStatement { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Links authorities (Police/Fire) to FNOL records
    /// </summary>
    public class FnolAuthority
    {
        public long FnolAuthorityId { get; set; }
        public long FnolId { get; set; }
        public long EntityId { get; set; }                  // Legacy FK to EntityMaster
        public long? VendorId { get; set; }                 // FK to VendorMaster
        public string? AuthorityType { get; set; }           // Police Department, Fire Station
        public string? ReportNumber { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Audit trail for sub-claim close/reopen operations
    /// </summary>
    public class SubClaimAudit
    {
        public long SubClaimAuditId { get; set; }
        public long SubClaimId { get; set; }
        public string Action { get; set; } = string.Empty;    // "Close" or "Reopen"
        public string? Reason { get; set; }                    // Close/Reopen reason
        public string? Remarks { get; set; }                   // User remarks
        public decimal? PreviousExpenseReserve { get; set; }   // Reserve before action
        public decimal? PreviousIndemnityReserve { get; set; } // Reserve before action
        public decimal? NewExpenseReserve { get; set; }        // Reserve after action
        public decimal? NewIndemnityReserve { get; set; }      // Reserve after action
        public string? PerformedBy { get; set; }               // User who performed the action
        public DateTime AuditDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Property damage records linked to FNOL
    /// </summary>
    public class FnolPropertyDamage
    {
        public long FnolPropertyDamageId { get; set; }
        public long FnolId { get; set; }
        public string? ClaimNumber { get; set; }
        public string? PropertyType { get; set; }             // Building, Fence, Other
        public string? PropertyDescription { get; set; }       // Property description

        // Property Owner Information
        public string? OwnerName { get; set; }
        public long? OwnerEntityId { get; set; }              // FK to EntityMaster
        public string? OwnerPhone { get; set; }
        public string? OwnerEmail { get; set; }
        public string? OwnerAddress { get; set; }
        public string? OwnerAddress2 { get; set; }
        public string? OwnerCity { get; set; }
        public string? OwnerState { get; set; }
        public string? OwnerZipCode { get; set; }

        // Property Location (can be different from owner address)
        public string? PropertyLocation { get; set; }
        public string? PropertyAddress { get; set; }
        public string? PropertyCity { get; set; }
        public string? PropertyState { get; set; }
        public string? PropertyZipCode { get; set; }

        public string? DamageDescription { get; set; }
        public decimal? EstimatedDamage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    /// <summary>
    /// Vendor master entity - separate from EntityMaster for performance
    /// Stores all vendors (Hospitals, Attorneys, Authorities, etc.)
    /// </summary>
    public class VendorMasterEntity
    {
        public long VendorId { get; set; }

        // Classification
        public string VendorType { get; set; } = string.Empty;  // Hospital, Defense Attorney, Plaintiff Attorney, Police Department, etc.
        public char EntityType { get; set; } = 'B';              // 'B' = Business, 'I' = Individual

        // Basic Info
        public string VendorName { get; set; } = string.Empty;
        public string? DoingBusinessAs { get; set; }
        public string? FEINNumber { get; set; }

        // Contact
        public string? ContactName { get; set; }
        public string? BusinessPhone { get; set; }
        public string? MobilePhone { get; set; }
        public string? FaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        // Dates
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }

        // Tax
        public bool W9Received { get; set; }
        public DateTime? W9ReceivedDate { get; set; }
        public bool SubjectTo1099 { get; set; }
        public bool BackupWithholding { get; set; }

        // Payment
        public bool ReceivesBulkPayment { get; set; }
        public string? PaymentFrequency { get; set; }
        public string? PaymentDays { get; set; }

        // Attorney-specific
        public string? BarNumber { get; set; }
        public string? BarState { get; set; }

        // Authority-specific
        public string? JurisdictionType { get; set; }

        // Status
        public char VendorStatus { get; set; } = 'Y';

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        // Legacy reference
        public long? LegacyEntityId { get; set; }

        // Navigation
        public virtual ICollection<VendorAddressEntity> Addresses { get; set; } = new List<VendorAddressEntity>();
    }

    /// <summary>
    /// Vendor Address entity
    /// </summary>
    public class VendorAddressEntity
    {
        public long VendorAddressId { get; set; }
        public long VendorId { get; set; }

        public char AddressType { get; set; } = 'M';  // 'M' = Main, 'A' = Alternate, 'T' = Temporary

        public string? StreetAddress { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; } = "USA";

        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }

        public char AddressStatus { get; set; } = 'Y';

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
