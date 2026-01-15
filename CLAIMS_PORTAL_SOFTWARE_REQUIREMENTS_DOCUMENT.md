# Claims Portal - Software Requirements Document (SRD)

## Document Information
| Item | Details |
|------|---------|
| **Project Name** | Claims Portal |
| **Version** | 1.0 |
| **Technology Stack** | .NET 10, Blazor Server, SQL Server, Bootstrap 5 |
| **Document Date** | January 2025 |
| **Status** | Production Ready |

---

## Table of Contents
1. [Executive Summary](#1-executive-summary)
2. [System Architecture](#2-system-architecture)
3. [Functional Requirements](#3-functional-requirements)
4. [Data Models](#4-data-models)
5. [User Interface Components](#5-user-interface-components)
6. [Database Schema](#6-database-schema)
7. [Services Layer](#7-services-layer)
8. [Security Requirements](#8-security-requirements)
9. [Deployment Guide](#9-deployment-guide)
10. [Testing Checklist](#10-testing-checklist)

---

## 1. Executive Summary

### 1.1 Purpose
The Claims Portal is a comprehensive insurance claims management system designed to handle the complete lifecycle of First Notice of Loss (FNOL) claims, from initial reporting through claim resolution.

### 1.2 Key Features
- **FNOL Management**: Complete workflow for creating, editing, and managing First Notice of Loss reports
- **Claim Inquiry**: Search and view claim details with comprehensive information tabs
- **Sub-Claims/Features Management**: Track individual coverage features within a claim
- **Vendor Management**: Maintain vendor/entity master records
- **Policy Integration**: Policy search and verification capabilities
- **Financial Tracking**: Reserve and payment tracking for claims

### 1.3 Target Users
- Claims Adjusters
- Claims Supervisors
- Customer Service Representatives
- Claims Administrators

---

## 2. System Architecture

### 2.1 Technology Stack
```
???????????????????????????????????????????????????????????????
?                    PRESENTATION LAYER                        ?
?  ????????????????????????????????????????????????????????????
?  ?              Blazor Server Components                    ??
?  ?  • Interactive Server Render Mode                        ??
?  ?  • Bootstrap 5 UI Framework                              ??
?  ?  • Bootstrap Icons                                       ??
?  ????????????????????????????????????????????????????????????
???????????????????????????????????????????????????????????????
?                     SERVICE LAYER                            ?
?  ????????????????????????????????????????????????????????????
?  ?  DatabaseClaimService    ?  DatabasePolicyService       ??
?  ?  DatabaseEntityService   ?  DatabaseLookupService       ??
?  ?  DatabaseVendorService   ?  AddressService              ??
?  ????????????????????????????????????????????????????????????
???????????????????????????????????????????????????????????????
?                      DATA LAYER                              ?
?  ????????????????????????????????????????????????????????????
?  ?              Entity Framework Core                       ??
?  ?              ClaimsPortalDbContext                       ??
?  ????????????????????????????????????????????????????????????
???????????????????????????????????????????????????????????????
?                     DATABASE                                 ?
?  ????????????????????????????????????????????????????????????
?  ?              SQL Server Database                         ??
?  ?              ClaimsPortal Schema                         ??
?  ????????????????????????????????????????????????????????????
???????????????????????????????????????????????????????????????
```

### 2.2 Project Structure
```
ClaimsPortal/
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor          # Main application layout
?   ?   ??? NavMenu.razor             # Navigation menu
?   ??? Pages/
?   ?   ??? Claim/
?   ?   ?   ??? ClaimDetail.razor     # Claim inquiry page
?   ?   ??? Fnol/
?   ?   ?   ??? FnolNew.razor         # FNOL wizard container
?   ?   ?   ??? FnolStep1_LossDetails.razor
?   ?   ?   ??? FnolStep2_PolicyAndInsured.razor
?   ?   ?   ??? FnolStep3_DriverAndInjury.razor
?   ?   ?   ??? FnolStep4_ThirdParties.razor
?   ?   ?   ??? FnolStep5_ReviewAndSave.razor
?   ?   ?   ??? FnolSearch.razor      # FNOL/Claim search
?   ?   ??? Dashboard.razor           # Main dashboard
?   ?   ??? VendorMaster.razor        # Vendor management
?   ??? Modals/
?   ?   ??? AttorneySearchModal.razor
?   ?   ??? AuthorityModal.razor
?   ?   ??? ClaimSuccessModal.razor
?   ?   ??? FeatureCreationModal.razor
?   ?   ??? PassengerModal.razor
?   ?   ??? PolicySearchModal.razor
?   ?   ??? PropertyDamageModal.razor
?   ?   ??? SubClaimModal.razor
?   ?   ??? ThirdPartyModal.razor
?   ?   ??? VendorDetailModal.razor
?   ?   ??? VendorViewModal.razor
?   ?   ??? WitnessModal.razor
?   ??? Shared/
?       ??? AddressSearchInput.razor
?       ??? AddressTemplate.razor
?       ??? AttorneyInfoForm.razor
?       ??? ClaimsLogPanel.razor
?       ??? CommunicationPanel.razor
?       ??? FinancialsPanel.razor
?       ??? InjuryInfoForm.razor
?       ??? InjuryTemplate.razor
?       ??? InjuryTemplateUnified.razor
?       ??? PartyInfoForm.razor
?       ??? SectionCard.razor
?       ??? SubClaimsSummaryGrid.razor
?       ??? SubClaimsSummarySection.razor
?       ??? WorkingSubClaimsGrid.razor
??? Data/
?   ??? ClaimsPortalDbContext.cs      # Entity Framework DbContext
??? Database/
?   ??? 000_CreateLiveNewDatabase.sql
?   ??? 001_InitialSchema.sql
?   ??? 002_VendorMaster_SchemaUpdates.sql
?   ??? 003_Vehicles_SchemaUpdates.sql
?   ??? 004_Fix_ClaimNumber_UniqueConstraint.sql
?   ??? 005_Add_PolicyDetails_ToFNOL.sql
??? Models/
?   ??? Address.cs
?   ??? Claim.cs
?   ??? Injury.cs
?   ??? Policy.cs
?   ??? Enums/
?   ?   ??? VendorEnums.cs
?   ??? Vendor/
?       ??? Vendor.cs
?       ??? VendorAddress.cs
?       ??? VendorContact.cs
?       ??? VendorPayment.cs
??? Services/
?   ??? AddressService.cs
?   ??? ClaimService.cs
?   ??? DatabaseClaimService.cs
?   ??? DatabaseEntityService.cs
?   ??? DatabaseLookupService.cs
?   ??? DatabasePolicyService.cs
?   ??? DatabaseVendorService.cs
?   ??? PolicyService.cs
?   ??? VendorService.cs
??? wwwroot/
?   ??? app.css                       # Custom styles
??? appsettings.json
??? Program.cs                        # Application entry point
```

---

## 3. Functional Requirements

### 3.1 FNOL (First Notice of Loss) Module

#### FR-3.1.1 FNOL Creation Wizard
The system shall provide a 5-step wizard for creating new FNOL records:

| Step | Name | Description |
|------|------|-------------|
| 1 | Loss Details | Capture date, time, location, cause, witnesses, authorities |
| 2 | Policy & Insured | Policy verification, insured party info, vehicle details |
| 3 | Driver & Injury | Driver information, injury details, passengers |
| 4 | Third Parties | Third party vehicles, pedestrians, property damage |
| 5 | Review & Save | Review all information, create sub-claims, submit |

#### FR-3.1.2 Loss Details (Step 1)
- Date of Loss (required)
- Time of Loss (required)
- Report Date (auto-populated, editable)
- Report Time (auto-populated, editable)
- Loss Location with address search
- Secondary Location (optional)
- Cause of Loss (dropdown)
- Weather Conditions (dropdown)
- Loss Description (multi-line text)
- Reported By (dropdown: Insured, Broker, Driver, Claimant, Other)
- Reporting Method (dropdown: Phone, Email, Mail, Fax, Agent Portal, Insured Portal)
- Witnesses (add multiple with modal)
- Authorities Notified (add multiple with modal)
- Flags: Other Vehicles Involved, Injuries, Property Damage

#### FR-3.1.3 Policy & Insured (Step 2)
- Policy Search Modal with verification
- Policy Information Display:
  - Policy Number
  - Status (Active/Inactive/Cancelled)
  - Effective Date
  - Expiration Date
  - Renewal Number
- Insured Party Information:
  - Name
  - Address (unified address template)
  - Phone Number
  - Email
  - FEIN/SS Number
- Insured Vehicle Selection:
  - VIN
  - Year/Make/Model
  - License Plate Number & State
  - Coverage limits display
- Vehicle Damage Information:
  - Is Damaged (Y/N)
  - Is Drivable (Y/N)
  - Was Towed (Y/N)
  - In Storage (Y/N with location)
  - Has Dash Cam (Y/N)
  - Damage Details (multi-line)
  - Roll Over, Water Damage, Headlights, Airbag flags

#### FR-3.1.4 Driver & Injury (Step 3)
- Driver Information:
  - Name
  - License Number & State
  - Date of Birth
  - Address
  - Phone & Email
  - FEIN/SS Number
  - Listed/Unlisted on Policy
- Driver Injury (if injured):
  - Injury Type (dropdown with 40+ options)
  - Severity Level (1-5 scale)
  - Injury Description
  - Fatality flag
  - Hospitalized flag with hospital details
- Attorney Representation:
  - Attorney Name
  - Law Firm
  - Address, Phone, Email
- Insured Passengers:
  - Add multiple via modal
  - Capture injury and attorney info per passenger

#### FR-3.1.5 Third Parties (Step 4)
- Third Party Types:
  - Vehicle (with vehicle details, owner, driver)
  - Pedestrian
  - Bicyclist
  - Property Owner
  - Other
- Per Third Party:
  - Name and contact information
  - Injury information (if applicable)
  - Attorney representation (if applicable)
  - Vehicle details (for vehicle type)
- Property Damage:
  - Add via dedicated modal
  - Property type, description, owner info, damage details

#### FR-3.1.6 Review & Save (Step 5)
- Summary display of all entered information
- Sub-Claims/Features creation:
  - Auto-generate based on coverage and claimants
  - Manual feature creation via modal
  - Assign adjusters
  - Set initial reserves
- Save as Draft functionality
- Submit Claim functionality
- Success modal with claim number display

### 3.2 Claim Inquiry Module

#### FR-3.2.1 Claim Search
- Search by:
  - Claim Number
  - FNOL Number
  - Policy Number
  - Date of Loss Range
  - Status
- Results grid with pagination
- Quick actions: View, Edit

#### FR-3.2.2 Claim Detail View
The system shall display claim information in tabbed format:

| Tab | Content |
|-----|---------|
| Loss Details | Date, time, location, cause, description, witnesses, authorities |
| Insured & Vehicle | Policy info, insured party, vehicle details |
| Driver & Passengers | Driver info, injury details, passenger list |
| Third Parties | Third party entities with details |
| Sub-Claims Summary | Feature grid with financial summary |
| Financials | Financial overview panel |
| Claims Log | Activity history |
| Communication | Communication records |

#### FR-3.2.3 Sub-Claims Grid
Display grid with columns:
- Checkbox (for multi-select)
- Features (Feature # + Coverage + Claimant Name)
- Limit ($)
- Deduct ($)
- Offset ($)
- Assigned Adjuster
- Reserves (Loss/Expense)
- Paid (Loss/Expense)
- Recovery Reserve (Salvage/Subrogation)
- Recovered ($)
- Status

Action Icons (when rows selected):
- Close Feature
- Loss Reserve
- Expense Reserve
- Reassign
- Loss Payment
- Expense Payment
- Salvage
- Subrogation
- Litigation
- Arbitration
- Negotiation

### 3.3 Vendor Management Module

#### FR-3.3.1 Vendor Master
- Create new vendors with:
  - Vendor Type (dropdown)
  - Business Name / Individual Name
  - Address (multiple)
  - Contact Information
  - Payment Methods (multiple: Check, ACH, Wire)
  - Tax Information (1099/W-9)
- View/Edit existing vendors
- Search vendors

---

## 4. Data Models

### 4.1 Core Claim Model
```csharp
public class Claim
{
    public string ClaimNumber { get; set; }
    public string PolicyNumber { get; set; }
    public string Status { get; set; }              // Open, Closed, Draft
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    
    public ClaimLossDetails LossDetails { get; set; }
    public Policy PolicyInfo { get; set; }
    public InsuredPartyInfo InsuredParty { get; set; }
    public VehicleInfo InsuredVehicle { get; set; }
    public DriverInfo Driver { get; set; }
    public Injury? DriverInjury { get; set; }
    public AttorneyInfo? DriverAttorney { get; set; }
    public List<InsuredPassenger> Passengers { get; set; }
    public List<ThirdParty> ThirdParties { get; set; }
    public List<PropertyDamage> PropertyDamages { get; set; }
    public List<SubClaim> SubClaims { get; set; }
}
```

### 4.2 Sub-Claim Model
```csharp
public class SubClaim
{
    public int Id { get; set; }
    public int FeatureNumber { get; set; }
    public string Coverage { get; set; }            // BI, PD, PIP, APIP, UM
    public string CoverageLimits { get; set; }
    public string ClaimantName { get; set; }
    public decimal ExpenseReserve { get; set; }
    public decimal IndemnityReserve { get; set; }
    public decimal ExpensePaid { get; set; }
    public decimal IndemnityPaid { get; set; }
    public decimal Deductible { get; set; }
    public decimal Offset { get; set; }
    public decimal SalvageReserve { get; set; }
    public decimal SubrogationReserve { get; set; }
    public decimal Recoveries { get; set; }
    public string AssignedAdjusterId { get; set; }
    public string AssignedAdjusterName { get; set; }
    public string Status { get; set; }              // Open, Closed, Reopened
    public string ClaimType { get; set; }           // Driver, Passenger, ThirdParty
}
```

### 4.3 Unified Injury Model
```csharp
public class Injury
{
    public string? InjuryType { get; set; }
    public int SeverityLevel { get; set; } = 1;     // 1-5 scale
    public string? InjuryDescription { get; set; }
    public bool IsFatality { get; set; }
    public bool WasTakenToHospital { get; set; }
    public string? HospitalName { get; set; }
    public string? HospitalStreetAddress { get; set; }
    public string? HospitalCity { get; set; }
    public string? HospitalState { get; set; }
    public string? HospitalZipCode { get; set; }
    public string? TreatingPhysician { get; set; }
}
```

### 4.4 Unified Address Model
```csharp
public class Address
{
    public string? StreetAddress { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? County { get; set; }
}
```

---

## 5. User Interface Components

### 5.1 Reusable Templates

| Component | Purpose | Location |
|-----------|---------|----------|
| `AddressTemplate.razor` | Unified address input with all optional fields | Shared |
| `InjuryTemplate.razor` | Injury information capture | Shared |
| `InjuryTemplateUnified.razor` | Enhanced injury form | Shared |
| `AttorneyInfoForm.razor` | Attorney information capture | Shared |
| `PartyInfoForm.razor` | Party/Person information capture | Shared |
| `SectionCard.razor` | Collapsible card section | Shared |
| `AddressSearchInput.razor` | Address autocomplete search | Shared |

### 5.2 Modal Components

| Modal | Purpose |
|-------|---------|
| `WitnessModal` | Add/edit witness information |
| `AuthorityModal` | Add/edit authority notification |
| `PassengerModal` | Add/edit passenger with injury/attorney |
| `ThirdPartyModal` | Add/edit third party (vehicle/pedestrian/etc.) |
| `PropertyDamageModal` | Add/edit property damage |
| `SubClaimModal` | Create/edit sub-claim features |
| `FeatureCreationModal` | Quick feature creation |
| `PolicySearchModal` | Search and select policy |
| `AttorneySearchModal` | Search and select attorney |
| `VendorDetailModal` | View vendor details |
| `VendorViewModal` | Extended vendor view |
| `ClaimSuccessModal` | Claim creation success confirmation |

### 5.3 Grid Components

| Component | Purpose |
|-----------|---------|
| `SubClaimsSummaryGrid` | 14-column sub-claims grid with actions |
| `WorkingSubClaimsGrid` | Working sub-claims detail view |
| `SubClaimsSummarySection` | Container with tabbed grid views |
| `FinancialsPanel` | Financial summary display |
| `ClaimsLogPanel` | Activity log display |
| `CommunicationPanel` | Communication records display |

---

## 6. Database Schema

### 6.1 Core Tables

```sql
-- FNOL Table (Primary Claims Table)
CREATE TABLE FNOL (
    FnolId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FnolNumber NVARCHAR(50) NOT NULL UNIQUE,
    ClaimNumber NVARCHAR(50) NULL,
    PolicyNumber NVARCHAR(50) NOT NULL,
    PolicyEffectiveDate DATE NULL,
    PolicyExpirationDate DATE NULL,
    PolicyCancelDate DATE NULL,
    PolicyStatus CHAR(1) NULL,
    InsuredName NVARCHAR(200) NULL,
    InsuredPhone NVARCHAR(20) NULL,
    RenewalNumber INT NULL,
    DateOfLoss DATE NOT NULL,
    TimeOfLoss TIME NULL,
    ReportDate DATE NOT NULL,
    ReportTime TIME NULL,
    LossLocation NVARCHAR(MAX) NOT NULL,
    LossLocation2 NVARCHAR(MAX) NULL,
    CauseOfLoss NVARCHAR(100) NULL,
    WeatherConditions NVARCHAR(100) NULL,
    LossDescription NVARCHAR(MAX) NOT NULL,
    HasVehicleDamage BIT DEFAULT 0,
    HasInjury BIT DEFAULT 0,
    HasPropertyDamage BIT DEFAULT 0,
    HasOtherVehiclesInvolved BIT DEFAULT 0,
    ReportedBy NVARCHAR(50) NULL,
    ReportedByName NVARCHAR(200) NULL,
    ReportedByPhone NVARCHAR(20) NULL,
    ReportedByEmail NVARCHAR(100) NULL,
    ReportedByEntityId BIGINT NULL,
    ReportingMethod NVARCHAR(50) NULL,
    FnolStatus CHAR(1) NOT NULL DEFAULT 'O',
    ClaimCreatedDate DATETIME NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedTime TIME NULL,
    CreatedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL,
    ModifiedTime TIME NULL,
    ModifiedBy NVARCHAR(100) NULL
);

-- SubClaim Table
CREATE TABLE SubClaim (
    SubClaimId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FnolId BIGINT NOT NULL FOREIGN KEY REFERENCES FNOL(FnolId),
    ClaimNumber NVARCHAR(50) NULL,
    SubClaimNumber NVARCHAR(50) NULL,
    FeatureNumber INT NOT NULL,
    ClaimantName NVARCHAR(200) NULL,
    ClaimantType NVARCHAR(50) NULL,
    Coverage NVARCHAR(20) NULL,
    CoverageLimits DECIMAL(18,2) NULL,
    AssignedAdjusterId BIGINT NULL,
    SubClaimStatus CHAR(1) NOT NULL DEFAULT 'O',
    OpenedDate DATETIME NULL,
    ClosedDate DATETIME NULL,
    IndemnityPaid DECIMAL(18,2) DEFAULT 0,
    IndemnityReserve DECIMAL(18,2) DEFAULT 0,
    ExpensePaid DECIMAL(18,2) DEFAULT 0,
    ExpenseReserve DECIMAL(18,2) DEFAULT 0,
    Reimbursement DECIMAL(18,2) DEFAULT 0,
    Recoveries DECIMAL(18,2) DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NULL
);

-- EntityMaster Table (Unified Party/Vendor)
CREATE TABLE EntityMaster (
    EntityId BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityType CHAR(1) NOT NULL,                    -- I=Individual, B=Business
    EntityGroupCode NVARCHAR(20) NOT NULL,          -- Vendor, Witness, Claimant
    PartyType NVARCHAR(50) NOT NULL,                -- Role/Type
    VendorType NVARCHAR(50) NULL,
    EntityName NVARCHAR(200) NOT NULL,
    DoingBusinessAs NVARCHAR(200) NULL,
    ContactName NVARCHAR(200) NULL,
    DateOfBirth DATE NULL,
    LicenseNumber NVARCHAR(50) NULL,
    LicenseState NVARCHAR(10) NULL,
    FEINorSS NVARCHAR(20) NULL,
    HomeBusinessPhone NVARCHAR(20) NULL,
    CellPhone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    EntityStatus CHAR(1) NOT NULL DEFAULT 'Y',
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NULL
);

-- Vehicle Table
CREATE TABLE Vehicle (
    VehicleId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FnolId BIGINT NOT NULL FOREIGN KEY REFERENCES FNOL(FnolId),
    ClaimNumber NVARCHAR(50) NULL,
    VehicleParty NVARCHAR(10) NOT NULL,             -- IPV, TPV
    VehicleListed BIT DEFAULT 1,
    VIN NVARCHAR(20) NULL,
    Make NVARCHAR(50) NULL,
    Model NVARCHAR(50) NULL,
    Year INT NULL,
    PlateNumber NVARCHAR(20) NULL,
    PlateState NVARCHAR(10) NULL,
    IsVehicleDamaged BIT DEFAULT 0,
    IsDrivable BIT DEFAULT 1,
    WasTowed BIT DEFAULT 0,
    IsInStorage BIT DEFAULT 0,
    StorageLocation NVARCHAR(200) NULL,
    HasDashCam BIT DEFAULT 0,
    DidVehicleRollOver BIT DEFAULT 0,
    HadWaterDamage BIT DEFAULT 0,
    HeadlightsWereOn BIT DEFAULT 0,
    AirbagDeployed BIT DEFAULT 0,
    DamageDetails NVARCHAR(MAX) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NULL
);

-- Claimant Table
CREATE TABLE Claimant (
    ClaimantId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FnolId BIGINT NOT NULL FOREIGN KEY REFERENCES FNOL(FnolId),
    ClaimNumber NVARCHAR(50) NULL,
    ClaimantEntityId BIGINT NULL FOREIGN KEY REFERENCES EntityMaster(EntityId),
    ClaimantName NVARCHAR(200) NULL,
    ClaimantType NVARCHAR(50) NULL,
    HasInjury BIT DEFAULT 0,
    InjuryType NVARCHAR(100) NULL,
    InjurySeverity NVARCHAR(20) NULL,
    InjuryDescription NVARCHAR(MAX) NULL,
    IsFatality BIT DEFAULT 0,
    IsHospitalized BIT DEFAULT 0,
    IsAttorneyRepresented BIT DEFAULT 0,
    AttorneyEntityId BIGINT NULL FOREIGN KEY REFERENCES EntityMaster(EntityId),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NULL
);

-- AddressMaster Table
CREATE TABLE AddressMaster (
    AddressId BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityId BIGINT NOT NULL FOREIGN KEY REFERENCES EntityMaster(EntityId),
    AddressType CHAR(1) NOT NULL DEFAULT 'M',       -- M=Main, S=Secondary
    StreetAddress NVARCHAR(200) NULL,
    Apt NVARCHAR(50) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(50) NULL,
    ZipCode NVARCHAR(20) NULL,
    County NVARCHAR(100) NULL,
    Country NVARCHAR(100) NULL DEFAULT 'USA',
    AddressStatus CHAR(1) NOT NULL DEFAULT 'Y',
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NULL
);

-- LookupCodes Table
CREATE TABLE LookupCodes (
    LookupId BIGINT IDENTITY(1,1) PRIMARY KEY,
    RecordType NVARCHAR(50) NOT NULL,
    RecordCode NVARCHAR(50) NOT NULL,
    RecordDesc NVARCHAR(200) NOT NULL,
    SortOrder INT NULL,
    RecordStatus CHAR(1) NOT NULL DEFAULT 'Y'
);
```

### 6.2 Relationship Diagram
```
???????????????       ????????????????
?    FNOL     ?????????   SubClaim   ?
???????????????       ????????????????
       ?
       ???????????????????????????????
       ?              ?              ?
       ?              ?              ?
??????????????? ???????????????? ?????????????????
?   Vehicle   ? ?   Claimant   ? ?  FnolWitness  ?
??????????????? ???????????????? ?????????????????
                      ?                   ?
                      ?                   ?
               ????????????????   ????????????????
               ? EntityMaster ?   ? EntityMaster ?
               ????????????????   ????????????????
                      ?
                      ?
               ????????????????
               ? AddressMaster?
               ????????????????
```

---

## 7. Services Layer

### 7.1 IDatabaseClaimService
```csharp
public interface IDatabaseClaimService
{
    Task<Fnol> CreateFnolAsync(Fnol fnol);
    Task<Fnol> GetFnolAsync(long fnolId);
    Task<Fnol?> GetFnolByNumberAsync(string fnolNumber);
    Task<Fnol?> GetClaimByNumberAsync(string claimNumber);
    Task<Models.Claim?> GetClaimWithDetailsAsync(string claimNumber);
    Task<List<Fnol>> GetOpenFnolsAsync();
    Task<List<Fnol>> GetDraftFnolsAsync(string? createdBy = null);
    Task<List<Fnol>> SearchFnolsAsync(...);
    Task<List<Fnol>> SearchClaimsAsync(...);
    Task<Fnol> UpdateFnolAsync(Fnol fnol);
    Task<Fnol> SaveFnolAsDraftAsync(Fnol fnol);
    Task<string> GenerateFnolNumberAsync();
    Task<string> GenerateClaimNumberAsync();
    Task<SubClaim> CreateSubClaimAsync(SubClaim subClaim);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task<Claimant> AddClaimantAsync(Claimant claimant);
    Task<Fnol> SaveCompleteFnolAsync(FnolSaveRequest request);
    Task<EntityMaster> CreateEntityAsync(EntityMaster entity);
    Task<AddressMaster> CreateAddressAsync(AddressMaster address);
    Task<FnolWitness> AddWitnessToFnolAsync(long fnolId, long entityId, string? statement);
    Task<FnolAuthority> AddAuthorityToFnolAsync(long fnolId, long entityId, string authorityType, string? reportNumber);
}
```

### 7.2 IDatabasePolicyService
```csharp
public interface IDatabasePolicyService
{
    Task<List<PolicyMaster>> SearchPoliciesAsync(string searchTerm);
    Task<PolicyMaster?> GetPolicyAsync(string policyNumber);
    Task<List<PolicyVehicle>> GetPolicyVehiclesAsync(long policyId);
    Task<List<PolicyDriver>> GetPolicyDriversAsync(long policyId);
    Task<List<PolicyCoverage>> GetPolicyCoveragesAsync(long policyId);
}
```

### 7.3 Service Registration
```csharp
// Program.cs
builder.Services.AddDbContext<ClaimsPortalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsPortal")));

builder.Services.AddScoped<IDatabaseClaimService, DatabaseClaimService>();
builder.Services.AddScoped<IDatabasePolicyService, DatabasePolicyService>();
builder.Services.AddScoped<IDatabaseEntityService, DatabaseEntityService>();
builder.Services.AddScoped<DatabaseLookupService>();
builder.Services.AddScoped<AddressService>();

// Legacy mock services (for backward compatibility)
builder.Services.AddScoped<IClaimService, MockClaimService>();
builder.Services.AddScoped<IPolicyService, MockPolicyService>();
builder.Services.AddScoped<IAdjusterService, MockAdjusterService>();
builder.Services.AddScoped<ILookupService, MockLookupService>();
```

---

## 8. Security Requirements

### 8.1 Authentication
- Integration with existing authentication system (to be configured)
- Session management via Blazor Server

### 8.2 Authorization
- Role-based access control (future implementation)
- Claim-level permissions (future implementation)

### 8.3 Data Protection
- SQL Server encryption at rest
- HTTPS for all communications
- Input validation on all forms
- Parameterized queries via Entity Framework

---

## 9. Deployment Guide

### 9.1 Prerequisites
- .NET 10 SDK
- SQL Server 2019 or later
- IIS or Kestrel web server

### 9.2 Database Setup
1. Execute `000_CreateLiveNewDatabase.sql` to create database
2. Execute `001_InitialSchema.sql` for core tables
3. Execute `002_VendorMaster_SchemaUpdates.sql`
4. Execute `003_Vehicles_SchemaUpdates.sql`
5. Execute `004_Fix_ClaimNumber_UniqueConstraint.sql`
6. Execute `005_Add_PolicyDetails_ToFNOL.sql`

### 9.3 Application Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    "ClaimsPortal": "Server=localhost;Database=ClaimsPortal;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 9.4 Build & Run
```bash
dotnet restore
dotnet build
dotnet run
```

---

## 10. Testing Checklist

### 10.1 FNOL Creation
- [ ] Create new FNOL with all steps completed
- [ ] Save as draft at each step
- [ ] Resume draft and complete
- [ ] Add witnesses and authorities
- [ ] Verify policy search works
- [ ] Add passengers with injuries
- [ ] Add third party vehicles
- [ ] Add pedestrians/bicyclists
- [ ] Add property damage
- [ ] Create sub-claims/features
- [ ] Submit and verify claim number generated

### 10.2 Claim Inquiry
- [ ] Search by claim number
- [ ] Search by FNOL number
- [ ] Search by policy number
- [ ] Search by date range
- [ ] View claim details - all tabs
- [ ] Verify sub-claims grid displays correctly
- [ ] Verify action icons appear when rows selected

### 10.3 Vendor Management
- [ ] Create new vendor
- [ ] Add multiple addresses
- [ ] Add payment methods
- [ ] Search vendors
- [ ] View vendor details

### 10.4 Data Integrity
- [ ] Verify all data saved to database
- [ ] Verify relationships maintained
- [ ] Verify lookup codes populate correctly

---

## Appendix A: Injury Types
The system supports 40+ injury types including:
- Abdominal/Internal, Amputation, Asphyxiation
- Back Injury, Broken Arm/Leg/Rib, Burns
- Chest Injury, Concussion, Contusion/Bruising
- Cut/Laceration, Dental Injury, Dislocation
- Eye Injury, Fracture, Head Injury
- Hearing Loss, Hernia, Internal Bleeding
- Joint Injury, Ligament Injury, Neck Injury
- Paralysis, Puncture Wound, Shoulder Injury
- Spinal Cord Injury, Sprain/Strain
- Traumatic Brain Injury, Whiplash, and more

## Appendix B: Coverage Types
| Code | Description |
|------|-------------|
| BI | Bodily Injury |
| PD | Property Damage |
| PIP | Personal Injury Protection |
| APIP | Additional PIP |
| UM | Uninsured Motorist |
| UIM | Underinsured Motorist |
| COLL | Collision |
| COMP | Comprehensive |
| MED | Medical Payments |

## Appendix C: Status Codes
| Code | Description |
|------|-------------|
| O | Open |
| C | Closed |
| D | Draft |
| R | Reopened |

---

**Document End**
