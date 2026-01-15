# ?? DATABASE RENAME GUIDE - LiveNew ? ClaimsPortal

## ?? Summary

**Difficulty Level**: ? VERY EASY
**Time Required**: 5 minutes
**Files to Change**: 2
**Lines to Change**: 3

---

## ? What's Already Done

I've already updated:
- ? `Database/000_CreateLiveNewDatabase.sql` - Renamed to use `ClaimsPortal`

---

## ?? What You Need to Do

### ONLY 1 File Remains: appsettings.json

#### Before (Current):
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

#### After (Updated):
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=ClaimsPortal;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

**Change**: Replace `LiveNew` with `ClaimsPortal` in the `Initial Catalog` parameter

---

## ?? Step-by-Step Implementation

### Step 1: Verify Updated SQL Script
```bash
File: Database/000_CreateLiveNewDatabase.sql
Status: ? ALREADY UPDATED
Check: Opens with ClaimsPortal instead of LiveNew
```

### Step 2: Update appsettings.json
```json
// Find this line:
"Initial Catalog=LiveNew;"

// Replace with:
"Initial Catalog=ClaimsPortal;"
```

### Step 3: Run SQL Scripts (in this order)
```
1. Database/000_CreateLiveNewDatabase.sql
   ? Creates ClaimsPortal database
   
2. Database/001_InitialSchema.sql
   ? Creates all tables, indexes, procedures in ClaimsPortal
```

### Step 4: Verify Connection
```sql
-- In SQL Server Management Studio:
-- Run this to verify database created:
SELECT name FROM sys.databases WHERE name = 'ClaimsPortal';

-- Should return: ClaimsPortal
```

### Step 5: Test Application
```csharp
// Application will now connect to:
// Server: HICD09062024\SQLEXPRESS
// Database: ClaimsPortal
// Everything else stays the same
```

---

## ?? Connection String Comparison

### Old (LiveNew):
```
Data Source=HICD09062024\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;...
```

### New (ClaimsPortal):
```
Data Source=HICD09062024\SQLEXPRESS;Initial Catalog=ClaimsPortal;Integrated Security=True;...
```

---

## ? Benefits of This Change

? **Avoids Conflict** - Your existing LiveNew database stays separate
? **Better Naming** - ClaimsPortal clearly identifies the purpose
? **No Code Changes** - Only configuration changes
? **No Risk** - Database name is just a reference
? **Easy Rollback** - Can change back anytime

---

## ?? Verification Checklist

After making changes:

- [ ] `Database/000_CreateLiveNewDatabase.sql` updated (? done for you)
- [ ] `appsettings.json` updated with `ClaimsPortal`
- [ ] SQL scripts executed successfully
- [ ] Database `ClaimsPortal` appears in SQL Server
- [ ] All 9 tables created in `ClaimsPortal`
- [ ] Application runs without connection errors
- [ ] FNOL data saves to `ClaimsPortal`

---

## ?? What Happens

### Before Running Scripts
```
Your existing databases:
- YOUR_POLICY_DB (Policy, Vehicle, Driver info)
- LiveNew (Other application)
- (other databases)
```

### After Running Updated Scripts
```
Your databases:
- YOUR_POLICY_DB (Policy, Vehicle, Driver info)
- LiveNew (Other application - unchanged)
- ClaimsPortal ? NEW (ClaimsPortal data)
```

---

## ?? Summary

| Item | Status | Action |
|------|--------|--------|
| SQL Create Script | ? Updated | No action needed |
| appsettings.json | ? Needs update | Replace `LiveNew` with `ClaimsPortal` |
| 001_InitialSchema.sql | ? No changes needed | Use as-is |
| Connection String | ? Needs update | Update database name |

---

## ?? Next Steps

1. **Update appsettings.json** (2 minutes)
   ```json
   Change: Initial Catalog=LiveNew
   To:     Initial Catalog=ClaimsPortal
   ```

2. **Run Updated SQL Scripts** (5 minutes)
   ```sql
   Script 1: 000_CreateLiveNewDatabase.sql (creates ClaimsPortal)
   Script 2: 001_InitialSchema.sql (creates tables)
   ```

3. **Verify in SQL Server** (1 minute)
   ```sql
   SELECT name FROM sys.databases WHERE name = 'ClaimsPortal';
   ```

4. **Test Application** (2 minutes)
   ```
   Run application and verify connection works
   ```

**Total Time: ~10 minutes**

---

## ?? Why This Works

The database name is **only** a reference in the connection string. Changing it:
- ? Doesn't affect any code logic
- ? Doesn't affect table structure
- ? Doesn't affect stored procedures
- ? Doesn't affect indexes
- ? Is completely reversible

The application uses the connection string to find the database. Update the connection string, and everything works with the new name.

---

## ?? Result

After these changes:
- ? Your ClaimsPortal uses database: `ClaimsPortal`
- ? Your existing `LiveNew` stays untouched
- ? No conflicts between databases
- ? Application runs normally
- ? All FNOL data saves correctly
- ? All 300+ concurrent users supported

---

**That's it! The rename is essentially a 2-minute configuration change.** ??

Need help updating appsettings.json? Let me know!
