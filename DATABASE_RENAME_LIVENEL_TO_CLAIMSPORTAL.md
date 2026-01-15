# ?? DATABASE RENAME - LiveNew ? ClaimsPortal

## ? VERY EASY - Simple Find & Replace

Changing the database name from `LiveNew` to `ClaimsPortal` is **trivial** and involves just updating a few configuration files.

---

## ?? Files That Need Changes

### 1. **SQL Schema Script** (001_InitialSchema.sql)
**Status**: Minimal change needed
**Impact**: None - this script creates tables, not the database

**Optional Change** (for clarity):
```sql
-- Add this comment at the top:
-- ============================================================================
-- LIVENEW DATABASE SCRIPT - TO BE RUN ON: ClaimsPortal DATABASE
-- Database Name: ClaimsPortal (instead of LiveNew)
-- ============================================================================
```

**Note**: The schema script itself doesn't create the database - it just creates tables inside whatever database you're using. No code changes needed.

### 2. **Database Creation Script** (000_CreateLiveNewDatabase.sql)
**Status**: Needs update
**Change**: 2 lines

**Before:**
```sql
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
```

**After:**
```sql
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'ClaimsPortal')
BEGIN
    CREATE DATABASE ClaimsPortal;
    PRINT 'Database ClaimsPortal created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ClaimsPortal already exists.';
END;

GO

USE ClaimsPortal;
```

### 3. **appsettings.json**
**Status**: Needs update
**Change**: 1 line in connection string

**Before:**
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=LiveNew;Integrated Security=True;..."
  }
}
```

**After:**
```json
{
  "ConnectionStrings": {
    "PolicyConnection": "YOUR_POLICY_DB_CONNECTION_STRING",
    "ClaimsConnection": "Data Source=HICD09062024\\SQLEXPRESS;Initial Catalog=ClaimsPortal;Integrated Security=True;..."
  }
}
```

### 4. **Any DbContext Configuration** (if you created one)
**Status**: Needs update if you have custom DbContext
**Change**: Connection string reference

---

## ?? Step-by-Step Implementation

### Step 1: Update 000_CreateLiveNewDatabase.sql
```bash
# Find all instances of 'LiveNew' and replace with 'ClaimsPortal'
```

Or manually:
```sql
-- Change these 3 lines:
CREATE DATABASE LiveNew;          ? CREATE DATABASE ClaimsPortal;
PRINT 'Database LiveNew created'  ? PRINT 'Database ClaimsPortal created'
USE LiveNew;                      ? USE ClaimsPortal;
```

### Step 2: Update appsettings.json
```json
"Initial Catalog=LiveNew;"  ?  "Initial Catalog=ClaimsPortal;"
```

### Step 3: Run the Updated SQL Scripts
```
1. Run: 000_CreateLiveNewDatabase.sql (creates ClaimsPortal)
2. Run: 001_InitialSchema.sql (creates tables in ClaimsPortal)
```

### Step 4: Verify
```sql
-- Verify database exists
SELECT name FROM sys.databases WHERE name = 'ClaimsPortal';

-- Verify tables created
USE ClaimsPortal;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
```

---

## ?? Complete Updated Files

### File: Database/000_CreateLiveNewDatabase.sql
