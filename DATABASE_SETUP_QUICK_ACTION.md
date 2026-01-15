# ?? DATABASE SETUP - IMMEDIATE ACTION ITEMS

## ? What's Ready

### 1. **LiveNew Database Script**
```
File: Database/000_CreateLiveNewDatabase.sql
Location: HICD09062024\SQLEXPRESS
Action: Ready to execute
```

### 2. **Complete Schema Script**
```
File: Database/001_InitialSchema.sql
Contains: 9 tables, 25+ indexes, 3 sequences, 2 procedures, 3 views
Location: HICD09062024\SQLEXPRESS ? LiveNew database
Status: Ready to execute
```

### 3. **Configuration Template**
```
File: appsettings.json
Status: Ready (needs Policy DB connection string)
```

### 4. **Setup Guides**
```
DATABASE_SETUP_DUAL_DATABASE_GUIDE.md - Overview of dual database setup
DATABASE_SETUP_COMPLETE_GUIDE.md - Step-by-step instructions
```

---

## ?? IMMEDIATE STEPS (Do This Now)

### Step 1: Open SQL Server Management Studio
```
Connect to: HICD09062024\SQLEXPRESS
Authentication: Windows or SQL (you know which one)
```

### Step 2: Create LiveNew Database
```sql
-- Copy and paste this into SSMS and execute:

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LiveNew')
BEGIN
    CREATE DATABASE LiveNew;
    PRINT 'Database LiveNew created successfully.';
END

GO

USE LiveNew;

SELECT DB_NAME() AS 'Current Database';
SELECT GETDATE() AS 'Current Time';
```

### Step 3: Run Schema Script
```
1. Open: Database/001_InitialSchema.sql
2. In SSMS, select all (Ctrl+A)
3. Execute (F5 or Execute button)
4. Wait for completion (2-3 minutes)
```

### Step 4: Verify Database
```sql
-- Run these to verify everything was created:

-- Check tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- Should show: AddressMaster, AuditLog, Claimants, EntityMaster, FNOL, 
--              LookupCodes, Policies, SubClaims, Vehicles

-- Check lookup data
SELECT COUNT(*) AS 'Total Lookup Codes' FROM LookupCodes;
-- Should show: 14
```

---

## ?? BLOCKING ITEM - NEED FROM YOU

### Policy Database Information

Please provide your **existing** policy database connection details:

```
Server Name: __________________________ (e.g., MY-SERVER, 192.168.1.100)
Database Name: _________________________ (e.g., PolicyDB, InsuranceDB)
Authentication: [ ] Windows [ ] SQL Server
Username (if SQL): _____________________
```

### Why We Need This
Your application has 2 databases:
1. **LiveNew** (HICD09062024\SQLEXPRESS) - Claims data ? We can create this ?
2. **Policy DB** (Your existing database) - Policy/vehicle/driver lookup ? Need connection info from you

---

## ?? Configuration (After DB is Created)

### appsettings.json (Update)
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING_HERE",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

### Example with Sample Values
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "Server=MY-INSURANCE-SERVER;Database=PolicyDB;Integrated Security=True;",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

---

## ?? Database Structure (After Running Script)

### LiveNew Database Will Have:

**9 Tables:**
```
? LookupCodes      - Reference data (14 initial records)
? Policies         - Insurance policies
? FNOL             - First Notice of Loss (main claims entry)
? Vehicles         - Vehicle damage information
? SubClaims        - Coverage/features per claim
? Claimants        - Injury parties and property owners
? EntityMaster     - Master parties/vendors
? AddressMaster    - Multiple addresses per entity
? AuditLog         - Complete transaction audit trail
```

**Automation Objects:**
```
? 3 Sequences (FNOL#, Claim#, Feature#)
? 2 Stored Procedures (Create FNOL, Finalize Claim)
? 3 Views (FNOL Summary, Entity with Address, Financial Summary)
? 25+ Indexes (optimized for 300 concurrent users)
```

---

## ? Connection Flow After Setup

```
User Application (Blazor)
    ?
    ??? Policy Database (Read-Only)
    ?   Server: YOUR-POLICY-SERVER
    ?   Purpose: Lookup policy, vehicle, driver info
    ?   Operations: SELECT only
    ?
    ??? Claims Database (Read-Write)
        Server: HICD09062024\SQLEXPRESS
        Database: LiveNew
        Purpose: Store FNOL, claims, injuries, audit trail
        Operations: INSERT, UPDATE, SELECT, DELETE
```

---

## ?? Success Criteria

After completing all steps, you should have:

- ? LiveNew database created on HICD09062024\SQLEXPRESS
- ? All 9 tables created with data
- ? 25+ indexes active
- ? 3 sequences ready for number generation
- ? 2 stored procedures available
- ? 3 views for queries
- ? 14 lookup codes inserted
- ? appsettings.json configured with both connection strings
- ? Application can connect to both databases
- ? FNOL workflow saves to LiveNew
- ? Policy lookup reads from Policy DB

---

## ?? Need Help?

### To Execute the Scripts:
1. You need **SQL Server Management Studio** (SSMS)
2. Connection to **HICD09062024\SQLEXPRESS**
3. Admin rights to create databases

### Providing Your Info:
Reply with:
```
Policy DB Server: ___________________
Policy DB Name: ___________________
Authentication: Windows [ ] SQL [ ]
```

---

## ?? Ready?

1. **Have you got SSMS open?** ? Start with Step 1
2. **Can you connect to HICD09062024\SQLEXPRESS?** ? Run Step 2-4
3. **Have your Policy DB details?** ? We can finish configuration

**I'm ready to help guide you through each step!** ??
