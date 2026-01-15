# ??? DATABASE SETUP - DUAL DATABASE CONFIGURATION

## Overview

Your ClaimsPortal will use **TWO databases**:

### Database 1: Policy Database (Read-Only Reference)
```
Purpose: Get insured policy, vehicle, and driver information
Status: Existing (you need to provide connection string)
Usage: Read-only queries for policy/vehicle/driver lookup
```

### Database 2: Claims Database (Read-Write)
```
Server: HICD09062024\SQLEXPRESS
Database: LiveNew
Purpose: Store all claims data entered and processed
Status: Will be created and populated
Usage: All FNOL and claim data storage
```

---

## ?? Database Configuration

### Connection String 1: Policy Database
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_EXISTING_CONNECTION_STRING_HERE"
  }
}
```

**What we need from you:**
- Server name
- Database name
- Authentication method (Windows/SQL)
- Example: `Server=SERVER_NAME;Database=POLICY_DB;Integrated Security=True;`

### Connection String 2: Claims Database (CONFIRMED)
```json
{
  "ConnectionStrings": {
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

---

## ?? Setup Steps

### Step 1: Verify SQL Server Access
```sql
-- Run on: HICD09062024\SQLEXPRESS
-- This will verify the connection works

SELECT @@SERVERNAME AS ServerName;
SELECT DB_NAME() AS DatabaseName;
SELECT GETDATE() AS CurrentDateTime;
```

### Step 2: Create LiveNew Database
```sql
-- Run on: HICD09062024\SQLEXPRESS
-- This will create the claims database

CREATE DATABASE LiveNew;
GO

USE LiveNew;
GO
```

### Step 3: Run Claims Schema Script
```
-- File: Database/001_InitialSchema.sql
-- Run on: HICD09062024\SQLEXPRESS - LiveNew database
-- This will create all tables, indexes, sequences, and procedures
```

### Step 4: Configure appsettings.json
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

### Step 5: Configure DbContext Services
```csharp
// In Program.cs
builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PolicyConnection"),
        sqlOptions => sqlOptions.CommandTimeout(30)
    )
);

builder.Services.AddDbContext<ClaimsPortalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ClaimsConnection"),
        sqlOptions => sqlOptions.CommandTimeout(30)
    )
);
```

---

## ? What I Need From You

To complete the database setup:

1. **Policy Database Connection String**
   - What's your existing database server?
   - What's the database name?
   - Example: `Server=MY-SERVER;Database=PolicyDb;Integrated Security=True;`

2. **Confirmation**
   - Can you access HICD09062024\SQLEXPRESS?
   - Do you have SQL Server Management Studio installed?
   - Can you create/modify databases on that server?

3. **User Account**
   - Are you using Windows Authentication?
   - Or SQL Server Authentication?

---

## ?? Database Structure Overview

### LiveNew Database (Claims Database)

**Core Tables:**
```
1. LookupCodes       - Reference data (party types, vendor types, etc.)
2. Policies          - Insurance policies
3. FNOL              - First Notice of Loss (main claim entry)
4. Vehicles          - Vehicle information
5. SubClaims         - Coverage/features per claim
6. Claimants         - Injury parties and property owners
7. EntityMaster      - Master parties/vendors
8. AddressMaster     - Addresses (multiple per entity)
9. AuditLog          - Complete audit trail
```

**Support Objects:**
```
Sequences:
- FNOLSequence (FNOL-1000001, FNOL-1000002, ...)
- ClaimNumberSequence (CLM-1, CLM-2, ...)
- SubClaimFeatureSequence (auto-number features)

Stored Procedures:
- sp_CreateFNOL (create new FNOL safely)
- sp_FinalizeClaim (finalize claim atomically)

Views:
- v_FNOLSummary (FNOL with counts)
- v_EntityWithAddress (Entity with main address)
- v_SubClaimFinancials (Financial summary)
```

---

## ?? Data Flow

```
???????????????????????????????????
?  Policy Database (Read-Only)    ?
?  (Existing - Policy/Vehicle/    ?
?   Driver Information)           ?
???????????????????????????????????
              ?
              ? Read: PolicyNumber
              ? Get: PolicyInfo
              ?
       ????????????????????
       ?  User searches   ?
       ?  policy in FNOL  ?
       ?  Step 2          ?
       ????????????????????
              ?
              ?
???????????????????????????????????
?  LiveNew Database (Read-Write)  ?
?  HICD09062024\SQLEXPRESS        ?
?                                 ?
?  Stores:                        ?
?  - FNOL data                    ?
?  - Vehicle damage               ?
?  - SubClaims (coverage)         ?
?  - Claimants (injuries)         ?
?  - Audit trail                  ?
???????????????????????????????????
```

---

## ?? Next Actions (Waiting on You)

- [ ] Provide Policy Database connection string
- [ ] Confirm SQL Server access to HICD09062024\SQLEXPRESS
- [ ] Confirm permission to create/modify databases

Once confirmed, I will:
1. ? Create the LiveNew database
2. ? Run the 001_InitialSchema.sql script
3. ? Create the DbContext configuration
4. ? Update Program.cs with dual database support
5. ? Provide connection string configuration
6. ? Create data access services

---

## ?? What's Ready

**Already Created:**
- ? 001_InitialSchema.sql (complete DDL for claims database)
- ? Table definitions (all 9 tables)
- ? Indexes (25+)
- ? Sequences (3 for number generation)
- ? Stored procedures (2 for key operations)
- ? Views (3 for common queries)
- ? Sample lookup data

**Waiting For:**
- ?? Policy Database connection string
- ?? Confirmation to proceed with LiveNew setup

---

## ?? Important Notes

1. **Two Separate Contexts**
   ```csharp
   // Policy Database (Read-Only)
   var policies = await _policyDbContext.Policies
       .Where(p => p.PolicyNumber == policyNumber)
       .FirstOrDefaultAsync();
   
   // Claims Database (Read-Write)
   var fnol = await _claimsDbContext.FNOLs
       .Where(f => f.PolicyNumber == policyNumber)
       .FirstOrDefaultAsync();
   ```

2. **Backup Strategy**
   - Both databases should be backed up regularly
   - LiveNew will grow as claims are added
   - Policy database is read-only, less critical

3. **Performance**
   - Each database has its own connection pool
   - Queries optimized for 300+ concurrent users
   - Both support your FNOL workflow

---

**Status**: Ready to proceed once you provide Policy Database details
**Action Required**: Please provide Policy Database connection string
**Timeline**: Setup can be completed within 1-2 hours
