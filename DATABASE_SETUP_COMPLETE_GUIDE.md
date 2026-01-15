# ?? DATABASE SETUP - COMPLETE STEP-BY-STEP GUIDE

## ?? Prerequisites Checklist

Before you start, verify:

- [ ] Can access HICD09062024\SQLEXPRESS
- [ ] SQL Server Management Studio (SSMS) is installed
- [ ] Have admin rights to create databases
- [ ] Know the connection string for your Policy database
- [ ] .NET 10 SDK is installed on development machine

---

## ?? Phase 1: Setup LiveNew Database (Claims Database)

### Step 1.1: Connect to SQL Server
```
1. Open SQL Server Management Studio (SSMS)
2. Server name: HICD09062024\SQLEXPRESS
3. Authentication: Windows Authentication (or SQL Auth if applicable)
4. Click Connect
```

### Step 1.2: Create LiveNew Database
```sql
-- Copy and paste into SSMS
-- Execute as individual query (NOT in a transaction)

-- Create database if not exists
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LiveNew')
BEGIN
    CREATE DATABASE LiveNew;
    PRINT 'Database LiveNew created successfully.';
END
ELSE
BEGIN
    PRINT 'Database LiveNew already exists.';
END;

GO

USE LiveNew;
GO

-- Verify
SELECT DB_NAME() AS CurrentDatabase;
SELECT GETDATE() AS Timestamp;
```

**Expected Result:**
```
Database LiveNew created successfully.
(or Database LiveNew already exists.)
```

### Step 1.3: Run Initial Schema Script
```
1. In SSMS, make sure you're connected to: HICD09062024\SQLEXPRESS
2. Make sure you're using database: LiveNew
3. Open file: Database/001_InitialSchema.sql
4. Execute entire script (Ctrl+A, then Execute)
```

**Expected Result:**
- All 9 tables created
- All indexes created
- All sequences created
- All stored procedures created
- Sample lookup data inserted

**Time to complete:** 2-3 minutes

### Step 1.4: Verify Database Setup

Run these verification queries to confirm everything is created:

```sql
-- Verify tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

-- Should show 9 tables:
-- AddressMaster
-- AuditLog
-- Claimants
-- EntityMaster
-- FNOL
-- LookupCodes
-- Policies
-- SubClaims
-- Vehicles
```

```sql
-- Verify sequences exist
SELECT * FROM sys.sequences;

-- Should show:
-- ClaimNumberSequence
-- FNOLSequence
-- SubClaimFeatureSequence
```

```sql
-- Verify stored procedures exist
SELECT * FROM sys.procedures 
WHERE name LIKE 'sp_%';

-- Should show:
-- sp_CreateFNOL
-- sp_FinalizeClaim
```

```sql
-- Verify sample lookup data
SELECT * FROM LookupCodes;

-- Should show 14 records of lookup data
```

---

## ?? Phase 2: Configure Policy Database Connection

### Step 2.1: Get Policy Database Information

You need to provide details about your existing Policy database:

```
Server: ____________________________
Database Name: ____________________________
Authentication: [ ] Windows [ ] SQL Server
Username (if SQL Auth): ____________________________
Password (if SQL Auth): ____________________________
```

### Step 2.2: Build Connection String

**For Windows Authentication:**
```
Server=YOUR_SERVER;Database=YOUR_DATABASE;Integrated Security=True;Connect Timeout=30;
```

**For SQL Server Authentication:**
```
Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Connect Timeout=30;
```

**Example:**
```
Server=MY-CLAIMS-SERVER;Database=PolicyDatabase;Integrated Security=True;
```

---

## ?? Phase 3: Update Application Configuration

### Step 3.1: Update appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DATABASE_CONNECTION_STRING_HERE",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

**Example with actual values:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PolicyConnection": "Server=CLAIMS-SERVER;Database=PolicyDB;Integrated Security=True;",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

### Step 3.2: Update Program.cs

Add these NuGet packages first:
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Update your `Program.cs`:
```csharp
// Add these using statements at the top
using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Data;

// Add these in your Program.cs before app.Build()
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

// Register services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IFnolService, FnolService>();
```

---

## ?? Phase 4: Test Database Connections

### Step 4.1: Create Connection Test Service

Create a new file: `Services/DatabaseHealthCheckService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Data;

namespace ClaimsPortal.Services;

public class DatabaseHealthCheckService
{
    private readonly PolicyDbContext _policyDb;
    private readonly ClaimsPortalDbContext _claimsDb;

    public DatabaseHealthCheckService(
        PolicyDbContext policyDb,
        ClaimsPortalDbContext claimsDb)
    {
        _policyDb = policyDb;
        _claimsDb = claimsDb;
    }

    public async Task<bool> CheckPolicyDatabaseAsync()
    {
        try
        {
            await _policyDb.Database.ExecuteSqlRawAsync("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckClaimsDatabaseAsync()
    {
        try
        {
            await _claimsDb.Database.ExecuteSqlRawAsync("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetDatabaseStatusAsync()
    {
        var policyDbOk = await CheckPolicyDatabaseAsync();
        var claimsDbOk = await CheckClaimsDatabaseAsync();

        return $"Policy DB: {(policyDbOk ? "? Connected" : "? Failed")}, Claims DB: {(claimsDbOk ? "? Connected" : "? Failed")}";
    }
}
```

### Step 4.2: Test in Dashboard

Create a health check in your Dashboard.razor:

```razor
@page "/dashboard"
@using ClaimsPortal.Services
@inject DatabaseHealthCheckService HealthCheck

<div class="alert alert-info">
    <h4>Database Status</h4>
    <p>@DbStatus</p>
</div>

@code {
    private string DbStatus = "Checking...";

    protected override async Task OnInitializedAsync()
    {
        DbStatus = await HealthCheck.GetDatabaseStatusAsync();
    }
}
```

---

## ? Verification Checklist

### LiveNew Database (Claims)
- [ ] Database created
- [ ] 9 tables created
- [ ] Indexes created
- [ ] Sequences created
- [ ] Stored procedures created
- [ ] Lookup data inserted

### Policy Database
- [ ] Connection string obtained
- [ ] Connection string tested
- [ ] Can query policy data
- [ ] Can read vehicle data
- [ ] Can read driver data

### Application Configuration
- [ ] appsettings.json updated
- [ ] Program.cs updated
- [ ] DbContext classes created
- [ ] NuGet packages installed
- [ ] Health check service created
- [ ] Application runs without errors
- [ ] Database connections working

---

## ?? Troubleshooting

### Issue: Cannot connect to HICD09062024\SQLEXPRESS

**Solution:**
```sql
-- Verify SQL Server is running
-- Check server name with:
SELECT @@SERVERNAME;

-- If connecting locally, try:
-- Server: (local)\SQLEXPRESS
-- Server: .\SQLEXPRESS
-- Server: HICD09062024\SQLEXPRESS
```

### Issue: Permission denied creating database

**Solution:**
- Verify you have admin rights
- Use "Run as Administrator" in SSMS
- Check SQL Server Authentication settings

### Issue: Cannot connect from application

**Solution:**
```csharp
// Test connection string
using (var connection = new SqlConnection(connectionString))
{
    try
    {
        connection.Open();
        Console.WriteLine("Connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection failed: {ex.Message}");
    }
}
```

### Issue: Timeout errors

**Solution:**
Increase timeout in connection string:
```
Connect Timeout=60;Command Timeout=60;
```

---

## ?? Database Summary After Setup

### LiveNew Database (HICD09062024\SQLEXPRESS)

**Tables Created:**
```
? LookupCodes       - Reference data
? Policies          - Insurance policies
? FNOL              - First Notice of Loss
? Vehicles          - Vehicle information
? SubClaims         - Coverage/features
? Claimants         - Injury parties
? EntityMaster      - Master parties/vendors
? AddressMaster     - Addresses
? AuditLog          - Audit trail
```

**Objects Created:**
```
? 25+ Indexes
? 3 Sequences
? 2 Stored Procedures
? 3 Views
? 14 Lookup Code Records
```

**Storage:**
```
Initial size: ~5 MB
Expected growth: 1-2 MB per 1000 claims
Backup strategy: Daily full, hourly log backups
```

---

## ?? Next Steps After Database Setup

1. **Create DbContext Classes**
   - PolicyDbContext (for policy database)
   - ClaimsPortalDbContext (for claims database)

2. **Create Data Access Services**
   - PolicyService
   - FnolService
   - ClaimService

3. **Update FNOL Workflow**
   - Step 2: PolicySearchModal to query Policy database
   - All steps: Save to Claims database

4. **Testing**
   - Test FNOL creation (saves to LiveNew)
   - Test policy search (reads from Policy DB)
   - Test concurrent access (300 users)

---

## ?? Support

If you need help:

1. **Connection Issues:** Provide exact server name and database name
2. **Schema Issues:** Check if 001_InitialSchema.sql ran completely
3. **Performance Issues:** Run query analyzer on slow queries
4. **Data Issues:** Check AuditLog table for transaction history

---

**Status**: Ready to proceed
**Timeline**: 30-45 minutes for complete setup
**Risk Level**: Very Low
**Difficulty**: Moderate (straightforward steps)

Let me know when you've completed each phase! ??
