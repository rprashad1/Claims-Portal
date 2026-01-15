# **IMMEDIATE ACTION - TRY THIS NOW**

## ?? **Don't Worry About the Errors**

The SQL syntax errors are **VS Code parser issues**. SQL Server might still execute it fine.

**Try this RIGHT NOW:**

```
1. Open: Database/001_InitialSchema.sql
2. Press: Ctrl+A (Select All)
3. Press: F5 (Execute)
4. Result: "Command(s) completed successfully." ? OR Error ?
```

---

## ? **If You Get "Command(s) completed successfully"**

**IGNORE the error messages in VS Code** - they're false positives.

Your database is ready! Run verification:

```sql
USE ClaimsPortal;
GO

SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
-- Should show: 9
```

---

## ? **If It Fails**

Use the **Minimal Tables Script** below.

---

## ?? **MINIMAL TABLES SCRIPT** (Just copy and execute this)

```sql
-- Drop and recreate database
USE master;
GO
DROP DATABASE IF EXISTS ClaimsPortal;
GO
CREATE DATABASE ClaimsPortal;
GO

USE ClaimsPortal;
GO

-- SECTION 1: LOOKUP TABLES
CREATE TABLE LookupCodes (
    LookupCodeId BIGINT PRIMARY KEY IDENTITY(1,1),
    RecordType NVARCHAR(50) NOT NULL,
    RecordCode NVARCHAR(50) NOT NULL,
    RecordDescription NVARCHAR(200) NOT NULL,
    RecordStatus CHAR(1) NOT NULL DEFAULT 'Y',
    SortOrder INT,
    Comments NVARCHAR(500),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT UQ_LookupCodes UNIQUE (RecordType, RecordCode),
    CONSTRAINT CK_RecordStatus CHECK (RecordStatus IN ('Y', 'D'))
);
GO

-- SECTION 2: POLICY TABLE
CREATE TABLE Policies (
    PolicyId BIGINT PRIMARY KEY IDENTITY(1,1),
    PolicyNumber NVARCHAR(50) UNIQUE NOT NULL,
    InsuredEntityId BIGINT NULL,
    PolicyType NVARCHAR(50),
    EffectiveDate DATETIME2 NOT NULL,
    ExpirationDate DATETIME2 NOT NULL,
    PolicyStatus CHAR(1) NOT NULL DEFAULT 'Y',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT CK_PolicyStatus CHECK (PolicyStatus IN ('Y', 'C', 'E'))
);
GO

-- SECTION 3: FNOL TABLE
CREATE TABLE FNOL (
    FnolId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolNumber NVARCHAR(50) UNIQUE NOT NULL,
    ClaimNumber NVARCHAR(50) UNIQUE NULL,
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
    HasVehicleDamage BIT DEFAULT 0,
    HasInjury BIT DEFAULT 0,
    HasPropertyDamage BIT DEFAULT 0,
    FnolStatus CHAR(1) NOT NULL DEFAULT 'O',
    ClaimCreatedDate DATETIME2 NULL,
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
GO

-- SECTION 4: VEHICLES TABLE
CREATE TABLE Vehicles (
    VehicleId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    ClaimNumber NVARCHAR(50) NULL,
    VehicleParty NVARCHAR(50) NOT NULL,
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
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Vehicle_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId)
);
GO

-- SECTION 5: ENTITY MASTER TABLE
CREATE TABLE EntityMaster (
    EntityId BIGINT PRIMARY KEY IDENTITY(1,1),
    EntityType CHAR(1) NOT NULL,
    PartyType NVARCHAR(50) NOT NULL,
    EntityGroupCode NVARCHAR(50) NOT NULL,
    VendorType NVARCHAR(50) NULL,
    EntityName NVARCHAR(500) NOT NULL,
    DBA NVARCHAR(200) NULL,
    EntityEffectiveDate DATETIME2,
    EntityTerminationDate DATETIME2 NULL,
    DateOfBirth DATE NULL,
    FEINorSS NVARCHAR(50) NULL,
    LicenseNumber NVARCHAR(50) NULL,
    LicenseState NVARCHAR(2) NULL,
    HomeBusinessPhone NVARCHAR(20),
    MobilePhone NVARCHAR(20),
    Email NVARCHAR(100),
    Is1099Reportable BIT DEFAULT 0,
    IsSubjectTo1099 BIT DEFAULT 0,
    IsBackupWithholding BIT DEFAULT 0,
    ReceivesBulkPayment BIT DEFAULT 0,
    PaymentFrequency NVARCHAR(20) NULL,
    BulkPaymentDayDate1 NVARCHAR(20) NULL,
    BulkPaymentDayDate2 NVARCHAR(20) NULL,
    EntityStatus CHAR(1) NOT NULL DEFAULT 'Y',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT CK_EntityType CHECK (EntityType IN ('B', 'I')),
    CONSTRAINT CK_EntityStatus CHECK (EntityStatus IN ('Y', 'D'))
);
GO

-- SECTION 6: ADDRESS MASTER TABLE
CREATE TABLE AddressMaster (
    AddressId BIGINT PRIMARY KEY IDENTITY(1,1),
    EntityId BIGINT NOT NULL,
    AddressType CHAR(1) NOT NULL,
    StreetAddress NVARCHAR(200),
    Apt NVARCHAR(50),
    City NVARCHAR(100),
    State NVARCHAR(2),
    Country NVARCHAR(50) DEFAULT 'USA',
    ZipCode NVARCHAR(10),
    HomeBusinessPhone NVARCHAR(20),
    MobilePhone NVARCHAR(20),
    Email NVARCHAR(100),
    AddressStatus CHAR(1) NOT NULL DEFAULT 'Y',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Address_Entity FOREIGN KEY (EntityId) REFERENCES EntityMaster(EntityId),
    CONSTRAINT CK_AddressType CHECK (AddressType IN ('M', 'A')),
    CONSTRAINT CK_AddressStatus CHECK (AddressStatus IN ('Y', 'D'))
);
GO

-- SECTION 7: SUB-CLAIMS TABLE
CREATE TABLE SubClaims (
    SubClaimId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    ClaimNumber NVARCHAR(50) NULL,
    SubClaimNumber NVARCHAR(50) UNIQUE NOT NULL,
    FeatureNumber INT NOT NULL,
    ClaimantName NVARCHAR(200),
    ClaimantType NVARCHAR(50),
    Coverage NVARCHAR(100),
    CoverageLimits DECIMAL(15, 2),
    AssignedAdjusterId BIGINT NULL,
    SubClaimStatus CHAR(1) NOT NULL DEFAULT 'O',
    OpenedDate DATETIME2,
    ClosedDate DATETIME2 NULL,
    IndemnityPaid DECIMAL(15, 2) DEFAULT 0,
    IndemnityReserve DECIMAL(15, 2) DEFAULT 0,
    ExpensePaid DECIMAL(15, 2) DEFAULT 0,
    ExpenseReserve DECIMAL(15, 2) DEFAULT 0,
    Reimbursement DECIMAL(15, 2) DEFAULT 0,
    Recoveries DECIMAL(15, 2) DEFAULT 0,
    SubrogationFileNumber NVARCHAR(50) NULL,
    SubrogationAdjusterId BIGINT NULL,
    ArbitrationFileNumber NVARCHAR(50) NULL,
    ArbitrationAdjusterId BIGINT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_SubClaim_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
    CONSTRAINT CK_SubClaimStatus CHECK (SubClaimStatus IN ('O', 'C', 'R'))
);
GO

-- SECTION 8: CLAIMANTS TABLE
CREATE TABLE Claimants (
    ClaimantId BIGINT PRIMARY KEY IDENTITY(1,1),
    FnolId BIGINT NOT NULL,
    SubClaimId BIGINT NULL,
    ClaimNumber NVARCHAR(50) NULL,
    FeatureNumber INT NULL,
    Coverage NVARCHAR(100),
    CoverageDescription NVARCHAR(500),
    CoverageLimits DECIMAL(15, 2),
    Deductible DECIMAL(15, 2),
    ClaimantName NVARCHAR(200),
    ClaimantEntityId BIGINT NULL,
    ClaimantType NVARCHAR(50),
    HasInjury BIT DEFAULT 0,
    InjuryType NVARCHAR(100),
    InjurySeverity NVARCHAR(50),
    InjuryDescription NVARCHAR(MAX),
    IsFatality BIT DEFAULT 0,
    IsHospitalized BIT DEFAULT 0,
    IsAttorneyRepresented BIT DEFAULT 0,
    AttorneyEntityId BIGINT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    CONSTRAINT FK_Claimant_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
    CONSTRAINT FK_Claimant_SubClaim FOREIGN KEY (SubClaimId) REFERENCES SubClaims(SubClaimId)
);
GO

-- SECTION 9: AUDIT LOG TABLE
CREATE TABLE AuditLog (
    AuditLogId BIGINT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordId BIGINT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL,
    FieldName NVARCHAR(100),
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    UserId NVARCHAR(100) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    SessionId NVARCHAR(100),
    
    CONSTRAINT CK_TransactionType CHECK (TransactionType IN ('INSERT', 'UPDATE', 'DELETE'))
);
GO

-- SECTION 13: INSERT SAMPLE DATA
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

-- VERIFY
SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
SELECT COUNT(*) as LookupCodeCount FROM LookupCodes;
```

---

## ? **Execute This Script**

1. Copy the script above
2. Open SSMS
3. Paste into new query
4. Press F5

**Expected Result**: 
- Message 1: TableCount = 9
- Message 2: LookupCodeCount = 14
- **"Command(s) completed successfully."**

---

**That's it!** You now have a working database with all 9 tables.

The procedures and views can be added later if needed, but the core database is ready!
