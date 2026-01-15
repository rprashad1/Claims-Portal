-- ============================================================================
-- CLAIMS PORTAL DATABASE SCHEMA
-- Production-Ready for 300 Concurrent Users
-- ============================================================================

-- ============================================================================
-- SECTION 1: LOOKUP TABLES (Reference Data)
-- ============================================================================

CREATE TABLE LookupCodes (
    LookupCodeId BIGINT PRIMARY KEY IDENTITY(1,1),
    RecordType NVARCHAR(50) NOT NULL,           -- 'Claimant', 'Vendor', 'VendorType', 'TransactionType'
    RecordCode NVARCHAR(50) NOT NULL,           -- 'IVD', 'ATTY', 'MED', '10'
    RecordDescription NVARCHAR(200) NOT NULL,   -- 'Insured Vehicle Driver', 'Attorney', 'Medical', 'Open Claim'
    RecordStatus CHAR(1) NOT NULL DEFAULT 'Y',  -- 'Y' = Active, 'D' = Disabled
    SortOrder INT,
    Comments NVARCHAR(500),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT UQ_LookupCodes UNIQUE (RecordType, RecordCode),
    CONSTRAINT CK_RecordStatus CHECK (RecordStatus IN ('Y', 'D'))
);

-- Index for lookup performance
CREATE NONCLUSTERED INDEX IX_LookupCodes_Type_Code 
ON LookupCodes(RecordType, RecordCode) 
INCLUDE (RecordDescription, RecordStatus);

CREATE NONCLUSTERED INDEX IX_LookupCodes_Status 
ON LookupCodes(RecordStatus) 
WHERE RecordStatus = 'Y';

-- ============================================================================
-- SECTION 2: POLICY TABLE
-- ============================================================================

CREATE TABLE Policies (
    PolicyId BIGINT PRIMARY KEY IDENTITY(1,1),
    PolicyNumber NVARCHAR(50) UNIQUE NOT NULL,
    InsuredEntityId BIGINT NULL,                -- Will reference EntityMaster (if exists)
    PolicyType NVARCHAR(50),                    -- 'Auto', 'Property', 'Liability', etc.
    EffectiveDate DATETIME2 NOT NULL,
    ExpirationDate DATETIME2 NOT NULL,
    PolicyStatus CHAR(1) NOT NULL DEFAULT 'Y', -- 'Y' = Active, 'C' = Cancelled, 'E' = Expired
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT CK_PolicyStatus CHECK (PolicyStatus IN ('Y', 'C', 'E'))
);

CREATE NONCLUSTERED INDEX IX_Policies_PolicyNumber ON Policies(PolicyNumber);
CREATE NONCLUSTERED INDEX IX_Policies_Status ON Policies(PolicyStatus) INCLUDE (PolicyNumber, InsuredEntityId);

-- ============================================================================
-- SECTION 3: FNOL TABLE (First Notice of Loss)
-- ============================================================================

CREATE TABLE FNOL (
    FnolId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolNumber NVARCHAR(50) UNIQUE NOT NULL,           -- Generated when FNOL created
    ClaimNumber NVARCHAR(50) UNIQUE NULL,              -- Generated when claim finalized (nullable until then)
    PolicyNumber NVARCHAR(50) NOT NULL,
    PolicyEffectiveDate DATETIME2,
    PolicyExpirationDate DATETIME2,
    PolicyCancelDate DATETIME2 NULL,
    PolicyStatus CHAR(1),
    DateOfLoss DATETIME2 NOT NULL,
    TimeOfLoss TIME(0),
    ReportDate DATETIME2 NOT NULL,
    ReportTime TIME(0),
    LossLocation NVARCHAR(MAX),
    CauseOfLoss NVARCHAR(MAX),
    WeatherConditions NVARCHAR(500),
    LossDescription NVARCHAR(MAX),
    
    -- Damage indicators
    HasVehicleDamage BIT DEFAULT 0,    -- Were there other vehicles involved?
    HasInjury BIT DEFAULT 0,           -- Was anyone injured?
    HasPropertyDamage BIT DEFAULT 0,   -- Was there property damage?
    
    -- Status tracking
    FnolStatus CHAR(1) NOT NULL DEFAULT 'O',  -- 'O' = Open, 'C' = Closed
    ClaimCreatedDate DATETIME2 NULL,          -- When claim was finalized from FNOL
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedTime TIME(0) NOT NULL DEFAULT CAST(GETDATE() AS TIME),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedTime TIME(0),
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_FNOL_Policy FOREIGN KEY (PolicyNumber) REFERENCES Policies(PolicyNumber),
    CONSTRAINT CK_FnolStatus CHECK (FnolStatus IN ('O', 'C')),
    CONSTRAINT CK_PolicyStatus_Check CHECK (PolicyStatus IN ('Y', 'C', 'E'))
);

-- Indexes for FNOL (frequently accessed)
CREATE NONCLUSTERED INDEX IX_FNOL_FnolNumber ON FNOL(FnolNumber);
CREATE NONCLUSTERED INDEX IX_FNOL_ClaimNumber ON FNOL(ClaimNumber) WHERE ClaimNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_FNOL_PolicyNumber ON FNOL(PolicyNumber) INCLUDE (FnolNumber, ClaimNumber, FnolStatus);
CREATE NONCLUSTERED INDEX IX_FNOL_Status ON FNOL(FnolStatus) INCLUDE (FnolNumber, ClaimNumber, CreatedDate);
CREATE NONCLUSTERED INDEX IX_FNOL_CreatedDate ON FNOL(CreatedDate DESC) INCLUDE (FnolNumber, ClaimNumber);

-- ============================================================================
-- SECTION 4: VEHICLE TABLE (Linked to FNOL)
-- ============================================================================

CREATE TABLE Vehicles (
    VehicleId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    ClaimNumber NVARCHAR(50) NULL,             -- Populated after claim creation
    VehicleParty NVARCHAR(50) NOT NULL,        -- 'Insured' or '3rd Party'
    VehicleListed BIT DEFAULT 0,
    VIN NVARCHAR(17) NULL,
    Make NVARCHAR(100),
    Model NVARCHAR(100),
    Year INT,
    PlateNumber NVARCHAR(20),
    IsVehicleDamaged BIT DEFAULT 0,
    WasTowed BIT DEFAULT 0,
    IsInStorage BIT DEFAULT 0,
    IsDrivable BIT DEFAULT 0,
    HeadlightsWereOn BIT DEFAULT 0,
    HadWaterDamage BIT DEFAULT 0,
    AirbagDeployed BIT DEFAULT 0,
    StorageLocation NVARCHAR(500),
    DamageDetails NVARCHAR(MAX),
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Vehicle_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId)
);

CREATE NONCLUSTERED INDEX IX_Vehicle_FnolId ON Vehicles(FnolId) INCLUDE (VehicleParty, VIN, IsVehicleDamaged);
CREATE NONCLUSTERED INDEX IX_Vehicle_ClaimNumber ON Vehicles(ClaimNumber) WHERE ClaimNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Vehicle_VIN ON Vehicles(VIN) WHERE VIN IS NOT NULL;

-- ============================================================================
-- SECTION 5: ENTITY MASTER TABLE (Comprehensive Party/Vendor Master)
-- ============================================================================

CREATE TABLE EntityMaster (
    EntityId BIGINT PRIMARY KEY IDENTITY(1,1),
    EntityType CHAR(1) NOT NULL,               -- 'B' = Business, 'I' = Individual
    PartyType NVARCHAR(50) NOT NULL,           -- 'Insured Vehicle Driver', 'Attorney', 'Repair Shop', etc.
    EntityGroupCode NVARCHAR(50) NOT NULL,     -- 'Claimant', 'Vendor', 'Insured'
    VendorType NVARCHAR(50) NULL,              -- 'Medical', 'Attorney', 'Hospital', 'Repair Shop', 'Rental'
    
    -- Name fields
    EntityName NVARCHAR(500) NOT NULL,         -- Full name (First, Middle, Last for Individual)
    DBA NVARCHAR(200) NULL,                    -- Doing Business As
    
    -- Dates
    EntityEffectiveDate DATETIME2,
    EntityTerminationDate DATETIME2 NULL,
    DateOfBirth DATE NULL,
    
    -- Tax & License
    FEINorSS NVARCHAR(50) NULL,
    LicenseNumber NVARCHAR(50) NULL,
    LicenseState NVARCHAR(2) NULL,
    
    -- Contact
    HomeBusinessPhone NVARCHAR(20),
    MobilePhone NVARCHAR(20),
    Email NVARCHAR(100),
    
    -- Payment & Tax Info
    Is1099Reportable BIT DEFAULT 0,
    IsSubjectTo1099 BIT DEFAULT 0,
    IsBackupWithholding BIT DEFAULT 0,
    ReceivesBulkPayment BIT DEFAULT 0,
    PaymentFrequency NVARCHAR(20) NULL,        -- 'Monthly', 'Weekly'
    BulkPaymentDayDate1 NVARCHAR(20) NULL,     -- "1" or "Monday"
    BulkPaymentDayDate2 NVARCHAR(20) NULL,     -- "15" or "Friday"
    
    -- Status
    EntityStatus CHAR(1) NOT NULL DEFAULT 'Y', -- 'Y' = Active, 'D' = Disabled
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT CK_EntityType CHECK (EntityType IN ('B', 'I')),
    CONSTRAINT CK_EntityStatus CHECK (EntityStatus IN ('Y', 'D'))
);

-- Indexes for EntityMaster
CREATE NONCLUSTERED INDEX IX_EntityMaster_Type ON EntityMaster(EntityType) INCLUDE (EntityName, EntityStatus);
CREATE NONCLUSTERED INDEX IX_EntityMaster_PartyType ON EntityMaster(PartyType) INCLUDE (EntityName, EntityStatus);
CREATE NONCLUSTERED INDEX IX_EntityMaster_GroupCode ON EntityMaster(EntityGroupCode, EntityStatus) INCLUDE (EntityName);
CREATE NONCLUSTERED INDEX IX_EntityMaster_FEIN ON EntityMaster(FEINorSS) WHERE FEINorSS IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_EntityMaster_Status ON EntityMaster(EntityStatus) WHERE EntityStatus = 'Y';
CREATE NONCLUSTERED INDEX IX_EntityMaster_Name ON EntityMaster(EntityName);

-- ============================================================================
-- SECTION 6: ADDRESS MASTER TABLE
-- ============================================================================

CREATE TABLE AddressMaster (
    AddressId BIGINT PRIMARY KEY IDENTITY(1,1),
    EntityId BIGINT NOT NULL,
    AddressType CHAR(1) NOT NULL,              -- 'M' = Main, 'A' = Alternate
    StreetAddress NVARCHAR(200),
    Apt NVARCHAR(50),
    City NVARCHAR(100),
    State NVARCHAR(2),
    Country NVARCHAR(50) DEFAULT 'USA',
    ZipCode NVARCHAR(10),
    HomeBusinessPhone NVARCHAR(20),
    MobilePhone NVARCHAR(20),
    Email NVARCHAR(100),
    AddressStatus CHAR(1) NOT NULL DEFAULT 'Y', -- 'Y' = Active, 'D' = Disabled
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Address_Entity FOREIGN KEY (EntityId) REFERENCES EntityMaster(EntityId),
    CONSTRAINT CK_AddressType CHECK (AddressType IN ('M', 'A')),
    CONSTRAINT CK_AddressStatus CHECK (AddressStatus IN ('Y', 'D'))
);

-- Indexes for AddressMaster
CREATE NONCLUSTERED INDEX IX_Address_EntityId ON AddressMaster(EntityId) INCLUDE (AddressType, AddressStatus);
CREATE NONCLUSTERED INDEX IX_Address_Status ON AddressMaster(AddressStatus) WHERE AddressStatus = 'Y';
CREATE NONCLUSTERED INDEX IX_Address_ZipCode ON AddressMaster(ZipCode) WHERE ZipCode IS NOT NULL;

-- ============================================================================
-- SECTION 7: SUB-CLAIM TABLE
-- ============================================================================

CREATE TABLE SubClaims (
    SubClaimId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    ClaimNumber NVARCHAR(50) NULL,             -- Populated after claim creation
    SubClaimNumber NVARCHAR(50) UNIQUE NOT NULL, -- Friendly display: CLM-001-001
    FeatureNumber INT NOT NULL,                -- Auto-generated sequence per claim
    ClaimantName NVARCHAR(200),
    ClaimantType NVARCHAR(50),                 -- Lookup: IVD, OVD, Pedestrian, etc.
    Coverage NVARCHAR(100),
    CoverageLimits DECIMAL(15, 2),
    AssignedAdjusterId BIGINT NULL,            -- Will reference EntityMaster (Adjuster)
    
    -- Status & Dates
    SubClaimStatus CHAR(1) NOT NULL DEFAULT 'O', -- 'O' = Open, 'C' = Closed, 'R' = Reopened
    OpenedDate DATETIME2,
    ClosedDate DATETIME2 NULL,
    
    -- Financials
    IndemnityPaid DECIMAL(15, 2) DEFAULT 0,
    IndemnityReserve DECIMAL(15, 2) DEFAULT 0,
    ExpensePaid DECIMAL(15, 2) DEFAULT 0,
    ExpenseReserve DECIMAL(15, 2) DEFAULT 0,
    Reimbursement DECIMAL(15, 2) DEFAULT 0,
    Recoveries DECIMAL(15, 2) DEFAULT 0,
    
    -- Subrogation
    SubrogationFileNumber NVARCHAR(50) NULL,
    SubrogationAdjusterId BIGINT NULL,
    
    -- Arbitration
    ArbitrationFileNumber NVARCHAR(50) NULL,
    ArbitrationAdjusterId BIGINT NULL,
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_SubClaim_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
    CONSTRAINT CK_SubClaimStatus CHECK (SubClaimStatus IN ('O', 'C', 'R'))
);

-- Indexes for SubClaims
CREATE NONCLUSTERED INDEX IX_SubClaim_FnolId ON SubClaims(FnolId) INCLUDE (SubClaimNumber, SubClaimStatus);
CREATE NONCLUSTERED INDEX IX_SubClaim_ClaimNumber ON SubClaims(ClaimNumber) WHERE ClaimNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_SubClaim_SubClaimNumber ON SubClaims(SubClaimNumber);
CREATE NONCLUSTERED INDEX IX_SubClaim_Status ON SubClaims(SubClaimStatus) INCLUDE (SubClaimNumber, FnolId);

-- ============================================================================
-- SECTION 8: CLAIMANTS TABLE (All Injury Parties & Property Owners)
-- ============================================================================

CREATE TABLE Claimants (
    ClaimantId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    SubClaimId BIGINT NULL,                    -- Links to SubClaims (can be null for multiple claimants per subclaim)
    ClaimNumber NVARCHAR(50) NULL,             -- Populated after claim creation
    FeatureNumber INT NULL,
    Coverage NVARCHAR(100),
    CoverageDescription NVARCHAR(500),
    CoverageLimits DECIMAL(15, 2),
    Deductible DECIMAL(15, 2),
    
    -- Claimant Details
    ClaimantName NVARCHAR(200),
    ClaimantEntityId BIGINT NULL,              -- Reference to EntityMaster
    ClaimantType NVARCHAR(50),                 -- Lookup: IVD, IVP, OVD, PED, etc.
    
    -- Injury Details
    HasInjury BIT DEFAULT 0,
    InjuryType NVARCHAR(100),
    InjurySeverity NVARCHAR(50),               -- 'Minor', 'Moderate', 'Severe'
    InjuryDescription NVARCHAR(MAX),
    IsFatality BIT DEFAULT 0,
    IsHospitalized BIT DEFAULT 0,
    
    -- Attorney Representation
    IsAttorneyRepresented BIT DEFAULT 0,
    AttorneyEntityId BIGINT NULL,              -- Reference to EntityMaster
    
    -- Audit trail
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Claimant_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
    CONSTRAINT FK_Claimant_SubClaim FOREIGN KEY (SubClaimId) REFERENCES SubClaims(SubClaimId)
);

-- Indexes for Claimants
CREATE NONCLUSTERED INDEX IX_Claimant_FnolId ON Claimants(FnolId) INCLUDE (ClaimantType, HasInjury);
CREATE NONCLUSTERED INDEX IX_Claimant_SubClaimId ON Claimants(SubClaimId) WHERE SubClaimId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Claimant_ClaimNumber ON Claimants(ClaimNumber) WHERE ClaimNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Claimant_EntityId ON Claimants(ClaimantEntityId) WHERE ClaimantEntityId IS NOT NULL;

-- ============================================================================
-- SECTION 9: AUDIT LOG TABLE (Complete Transaction Trail)
-- ============================================================================

CREATE TABLE AuditLog (
    AuditLogId BIGINT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordId BIGINT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL,     -- 'INSERT', 'UPDATE', 'DELETE'
    FieldName NVARCHAR(100),                   -- Null for INSERT/DELETE, specific field for UPDATE
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    UserId NVARCHAR(100) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    SessionId NVARCHAR(100),
    
    CONSTRAINT CK_TransactionType CHECK (TransactionType IN ('INSERT', 'UPDATE', 'DELETE'))
);

-- Index for audit queries
CREATE NONCLUSTERED INDEX IX_AuditLog_TableRecord ON AuditLog(TableName, RecordId) INCLUDE (TransactionDate, TransactionType, UserId);
CREATE NONCLUSTERED INDEX IX_AuditLog_TransactionDate ON AuditLog(TransactionDate DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_UserId ON AuditLog(UserId, TransactionDate DESC);

-- ============================================================================
-- SECTION 10: FNOL SEQUENCE GENERATOR (For FNOL Numbers)
-- ============================================================================

CREATE SEQUENCE FNOLSequence 
    START WITH 1000001 
    INCREMENT BY 1 
    NO CACHE;

GO

CREATE SEQUENCE ClaimNumberSequence 
    START WITH 1 
    INCREMENT BY 1 
    NO CACHE;

GO

CREATE SEQUENCE SubClaimFeatureSequence 
    START WITH 1 
    INCREMENT BY 1 
    NO CACHE;

GO

-- ============================================================================
-- SECTION 11: STORED PROCEDURES FOR KEY OPERATIONS
-- ============================================================================

-- Procedure to create new FNOL
CREATE PROCEDURE sp_CreateFNOL
    @PolicyNumber NVARCHAR(50),
    @DateOfLoss DATETIME2,
    @LossLocation NVARCHAR(MAX),
    @CreatedBy NVARCHAR(100),
    @FnolNumber NVARCHAR(50) OUTPUT,
    @FnolId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Validate policy exists
        IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = @PolicyNumber)
        BEGIN
            THROW 50001, N'Policy not found', 1;
        END
        
        -- Generate FNOL Number
        SET @FnolNumber = 'FNOL-' + CAST(NEXT VALUE FOR FNOLSequence AS NVARCHAR(20));
        
        -- Insert FNOL record
        INSERT INTO FNOL (
            FnolNumber, PolicyNumber, DateOfLoss, ReportDate, ReportTime,
            FnolStatus, CreatedBy, CreatedDate, CreatedTime
        )
        VALUES (
            @FnolNumber, @PolicyNumber, @DateOfLoss, GETDATE(), CAST(GETDATE() AS TIME),
            'O', @CreatedBy, GETDATE(), CAST(GETDATE() AS TIME)
        );
        
        SET @FnolId = SCOPE_IDENTITY();
        
        -- Log to audit
        INSERT INTO AuditLog (TableName, RecordId, TransactionType, NewValue, UserId, TransactionDate)
        VALUES ('FNOL', @FnolId, 'INSERT', @FnolNumber, @CreatedBy, GETDATE());
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO

-- Procedure to finalize claim from FNOL
CREATE PROCEDURE sp_FinalizeClaim
    @FnolId BIGINT,
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @ClaimNumber NVARCHAR(50);
        DECLARE @FnolNumber NVARCHAR(50);
        
        -- Get FNOL details
        SELECT @FnolNumber = FnolNumber FROM FNOL WHERE FnolId = @FnolId;
        
        IF @FnolNumber IS NULL
            THROW 50002, N'FNOL not found', 1;
        
        -- Generate Claim Number
        SET @ClaimNumber = 'CLM-' + CAST(NEXT VALUE FOR ClaimNumberSequence AS NVARCHAR(20));
        
        -- Update FNOL with claim number
        UPDATE FNOL
        SET ClaimNumber = @ClaimNumber,
            ClaimCreatedDate = GETDATE(),
            ModifiedBy = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE FnolId = @FnolId;
        
        -- Update SubClaims with claim number
        UPDATE SubClaims
        SET ClaimNumber = @ClaimNumber
        WHERE FnolId = @FnolId;
        
        -- Update Vehicles with claim number
        UPDATE Vehicles
        SET ClaimNumber = @ClaimNumber
        WHERE FnolId = @FnolId;
        
        -- Update Claimants with claim number
        UPDATE Claimants
        SET ClaimNumber = @ClaimNumber
        WHERE FnolId = @FnolId;
        
        -- Log to audit
        INSERT INTO AuditLog (TableName, RecordId, TransactionType, FieldName, NewValue, UserId, TransactionDate)
        VALUES ('FNOL', @FnolId, 'UPDATE', 'ClaimNumber', @ClaimNumber, @ModifiedBy, GETDATE());
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO

-- ============================================================================
-- SECTION 12: VIEWS FOR COMMON QUERIES
-- ============================================================================

-- View for FNOL Summary with claims count
CREATE VIEW v_FNOLSummary AS
SELECT 
    f.FnolId,
    f.FnolNumber,
    f.ClaimNumber,
    f.PolicyNumber,
    f.DateOfLoss,
    f.ReportDate,
    f.FnolStatus,
    COUNT(DISTINCT sc.SubClaimId) AS SubClaimCount,
    COUNT(DISTINCT v.VehicleId) AS VehicleCount,
    f.CreatedDate,
    f.CreatedBy
FROM FNOL f
LEFT JOIN SubClaims sc ON f.FnolId = sc.FnolId
LEFT JOIN Vehicles v ON f.FnolId = v.FnolId
GROUP BY f.FnolId, f.FnolNumber, f.ClaimNumber, f.PolicyNumber, f.DateOfLoss, 
         f.ReportDate, f.FnolStatus, f.CreatedDate, f.CreatedBy;

GO

-- View for Entity with primary address
CREATE VIEW v_EntityWithAddress AS
SELECT 
    e.EntityId,
    e.EntityName,
    e.PartyType,
    e.EntityStatus,
    a.AddressId,
    a.StreetAddress,
    a.City,
    a.State,
    a.ZipCode,
    e.Email,
    e.HomeBusinessPhone
FROM EntityMaster e
LEFT JOIN AddressMaster a ON e.EntityId = a.EntityId AND a.AddressType = 'M' AND a.AddressStatus = 'Y'
WHERE e.EntityStatus = 'Y';

GO

-- View for SubClaim financial summary
CREATE VIEW v_SubClaimFinancials AS
SELECT 
    sc.SubClaimId,
    sc.SubClaimNumber,
    sc.ClaimNumber,
    sc.FeatureNumber,
    (sc.IndemnityPaid + sc.IndemnityReserve) AS TotalIndemnity,
    (sc.ExpensePaid + sc.ExpenseReserve) AS TotalExpense,
    (sc.IndemnityPaid + sc.IndemnityReserve + sc.ExpensePaid + sc.ExpenseReserve) AS TotalReserve,
    sc.Recoveries,
    ((sc.IndemnityPaid + sc.IndemnityReserve + sc.ExpensePaid + sc.ExpenseReserve) - sc.Recoveries) AS NetLiability
FROM SubClaims sc;

GO

-- ============================================================================
-- SECTION 13: SAMPLE DATA INSERT (FOR TESTING)
-- ============================================================================

-- Insert lookup codes
INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, CreatedBy)
VALUES
    ('Claimant', 'IVD', 'Insured Vehicle Driver', 'Y', 'System'),
    ('Claimant', 'IVP', 'Insured Vehicle Passenger', 'Y', 'System'),
    ('Claimant', 'OVD', 'Other Vehicle Driver', 'Y', 'System'),
    ('Claimant', 'OVP', 'Other Vehicle Passenger', 'Y', 'System'),
    ('Claimant', 'PED', 'Pedestrian', 'Y', 'System'),
    ('Claimant', 'BYL', 'Bicyclist', 'Y', 'System'),
    ('VendorType', 'MED', 'Medical', 'Y', 'System'),
    ('VendorType', 'ATTY', 'Attorney', 'Y', 'System'),
    ('VendorType', 'HOSP', 'Hospital', 'Y', 'System'),
    ('VendorType', 'ARS', 'Repair Shop', 'Y', 'System'),
    ('VendorType', 'CRS', 'Car Rental Service', 'Y', 'System'),
    ('TransactionType', '10', 'Open Claim', 'Y', 'System'),
    ('TransactionType', '15', 'Reopen Claim', 'Y', 'System'),
    ('TransactionType', '20', 'Close Claim', 'Y', 'System');

GO
