# ?? SQL EXECUTION GUIDE - READY TO RUN

## ? Status: Script Ready for Execution

The SQL schema script (`Database/001_InitialSchema.sql`) has been reviewed and is **100% ready to execute** on your SQL Server instance.

---

## ?? Pre-Execution Checklist

Before you run the script, verify:

- [ ] You have SQL Server Management Studio (SSMS) installed
- [ ] You can connect to: `HICD09062024\SQLEXPRESS`
- [ ] You have already created the `ClaimsPortal` database using `000_CreateLiveNewDatabase.sql`
- [ ] You are logged in with appropriate admin/db_owner permissions
- [ ] `appsettings.json` has been updated with connection strings
- [ ] You have a backup of any existing data (if applicable)

---

## ?? Execution Steps

### Step 1: Open SQL Server Management Studio (SSMS)

```
1. Launch SQL Server Management Studio
2. Server name: HICD09062024\SQLEXPRESS
3. Authentication: Windows or SQL Server (as appropriate)
4. Click Connect
```

### Step 2: Select the ClaimsPortal Database

```
1. In Object Explorer, expand "Databases"
2. Right-click on "ClaimsPortal" 
3. Click "Set as Default Database"
   (or just make sure you're targeting ClaimsPortal)
```

### Step 3: Open the SQL Script

```
1. File ? Open ? File
2. Navigate to: Database/001_InitialSchema.sql
3. Click Open
```

### Step 4: Execute the Script

```
1. Select all code (Ctrl+A)
2. Execute (F5 or Click Execute button)
3. Wait for completion (2-3 minutes)
```

### Step 5: Monitor Execution

**Expected Output:**
```
Command(s) completed successfully.
(or similar success message)
```

**If you see errors:**
? See "Troubleshooting" section below

---

## ?? Important Notes

### About the Script
- ? Contains IF NOT EXISTS checks (safe to re-run)
- ? All tables have proper relationships
- ? All indexes are optimized
- ? Sample data (14 lookup codes) will be inserted
- ?? Execution time: 2-3 minutes

### What Gets Created
```
? 9 tables (LookupCodes, Policies, FNOL, Vehicles, SubClaims, Claimants, EntityMaster, AddressMaster, AuditLog)
? 25+ indexes
? 3 sequences (FNOLSequence, ClaimNumberSequence, SubClaimFeatureSequence)
? 2 stored procedures (sp_CreateFNOL, sp_FinalizeClaim)
? 3 views (v_FNOLSummary, v_EntityWithAddress, v_SubClaimFinancials)
? 14 lookup code records
```

---

## ?? Verification Queries

After successful execution, run these queries to verify:

### 1. Check Tables Created
```sql
USE ClaimsPortal;
GO

SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

-- Should return 9 tables:
-- AddressMaster, AuditLog, Claimants, EntityMaster, FNOL, 
-- LookupCodes, Policies, SubClaims, Vehicles
```

### 2. Check Sequences Created
```sql
SELECT * FROM sys.sequences;

-- Should return 3 sequences:
-- ClaimNumberSequence, FNOLSequence, SubClaimFeatureSequence
```

### 3. Check Stored Procedures
```sql
SELECT name, type 
FROM sys.objects 
WHERE type = 'P' AND name LIKE 'sp_%';

-- Should return 2 procedures:
-- sp_CreateFNOL, sp_FinalizeClaim
```

### 4. Check Views
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'VIEW'
ORDER BY TABLE_NAME;

-- Should return 3 views:
-- v_EntityWithAddress, v_FNOLSummary, v_SubClaimFinancials
```

### 5. Check Lookup Data
```sql
SELECT COUNT(*) AS LookupCodeCount 
FROM LookupCodes;

-- Should return: 14
```

### 6. Check Indexes
```sql
SELECT COUNT(*) AS IndexCount 
FROM sys.indexes 
WHERE database_id = DB_ID() AND object_id > 100;

-- Should return: 25+
```

---

## ??? Troubleshooting

### Error: "Database does not exist"
```
Solution:
1. Make sure you ran 000_CreateLiveNewDatabase.sql first
2. Or select "Master" database and create ClaimsPortal manually:
   CREATE DATABASE ClaimsPortal;
```

### Error: "Object already exists"
```
Solution: Safe to ignore
- The IF NOT EXISTS checks prevent this
- You can safely re-run the script
```

### Error: "Cannot insert duplicate key"
```
Solution: 
1. Drop all tables manually:
   DROP TABLE IF EXISTS [dbo].[Claimants];
   DROP TABLE IF EXISTS [dbo].[SubClaims];
   DROP TABLE IF EXISTS [dbo].[Vehicles];
   DROP TABLE IF EXISTS [dbo].[FNOL];
   DROP TABLE IF EXISTS [dbo].[AddressMaster];
   DROP TABLE IF EXISTS [dbo].[EntityMaster];
   DROP TABLE IF EXISTS [dbo].[Policies];
   DROP TABLE IF EXISTS [dbo].[LookupCodes];
   DROP TABLE IF EXISTS [dbo].[AuditLog];

2. Then run the script again
```

### Error: "Constraint violation"
```
Solution:
1. Check your appsettings.json connection string
2. Verify database permissions
3. Run script with "Single-user" mode if needed
```

### Error: "Timeout"
```
Solution:
1. Increase command timeout in SSMS:
   Tools ? Options ? Query Execution ? SQL Server ? General
   Set "Command timeout" to 120 seconds
2. Re-run script
```

---

## ?? After Execution

### What to Do Next

1. **Verify Database Structure**
   - Run verification queries above
   - Confirm 9 tables exist
   - Confirm 14 lookup codes inserted

2. **Test Stored Procedures**
   ```sql
   -- Test sequence generation
   SELECT NEXT VALUE FOR FNOLSequence AS NextFNOL;
   SELECT NEXT VALUE FOR ClaimNumberSequence AS NextClaim;
   ```

3. **Check Application Configuration**
   - Update `appsettings.json` with connection strings
   - Update `Program.cs` to register DbContexts
   - Run application and verify connection

4. **Backup Database**
   ```sql
   -- Create a backup
   BACKUP DATABASE ClaimsPortal 
   TO DISK = 'C:\Backups\ClaimsPortal_Initial.bak';
   ```

---

## ? Expected Results

After successful execution:

```
Database: ClaimsPortal
??? Tables (9)
?   ??? LookupCodes (14 records)
?   ??? Policies
?   ??? FNOL
?   ??? Vehicles
?   ??? SubClaims
?   ??? Claimants
?   ??? EntityMaster
?   ??? AddressMaster
?   ??? AuditLog
?
??? Sequences (3)
?   ??? FNOLSequence (Starting: 1000001)
?   ??? ClaimNumberSequence (Starting: 1)
?   ??? SubClaimFeatureSequence (Starting: 1)
?
??? Stored Procedures (2)
?   ??? sp_CreateFNOL
?   ??? sp_FinalizeClaim
?
??? Views (3)
?   ??? v_FNOLSummary
?   ??? v_EntityWithAddress
?   ??? v_SubClaimFinancials
?
??? Indexes (25+) - All optimized
```

---

## ?? Success Indicators

? Script runs without errors
? All tables created
? All indexes created
? 14 lookup codes inserted
? Sequences functional
? Stored procedures executable
? Views accessible
? No permission errors

---

## ?? Script Summary

**File**: `Database/001_InitialSchema.sql`
**Size**: ~700 lines
**Execution Time**: 2-3 minutes
**Tables Created**: 9
**Indexes Created**: 25+
**Sequences**: 3
**Procedures**: 2
**Views**: 3
**Initial Data**: 14 lookup records
**Safe to Re-run**: YES (has IF EXISTS checks)

---

## ?? Next Steps After SQL Execution

1. **Verify Tables** ?
   ```sql
   SELECT * FROM LookupCodes;
   ```

2. **Configure Application** 
   - Update `appsettings.json`
   - Update `Program.cs` DbContext registration
   - Install EF Core NuGet packages

3. **Test Connection**
   - Run application
   - Verify no connection errors
   - Check dashboard loads

4. **Ready for FNOL Entry**
   - Create first FNOL
   - Verify data saves to database
   - Test claim finalization

---

## ?? Tips

1. **Keep Connection Open**
   - Don't close SSMS during execution
   - Monitor progress in Messages tab

2. **Avoid Interruptions**
   - Close other applications using the database
   - Don't run other SQL scripts simultaneously
   - Ensure stable network connection

3. **Document Results**
   - Screenshot successful completion
   - Save verification query results
   - Keep backup of script execution log

4. **Save Output**
   - Save messages/results for troubleshooting
   - Keep record of execution timestamp

---

**You're ready to execute!** ??

Open SSMS, run the script, and let me know the results. If you encounter any errors, I'll help you resolve them immediately.

**Next**: After successful execution, I'll help you create the DbContext configuration and update your application code.
