# ?? **FNOL FOREIGN KEY ERROR - DIAGNOSIS & FIX**

## ? **Error Message**
```
Foreign key 'FK_FNOL_Policy' references invalid table 'Policies'
```

---

## ? **What's Actually Happening**

The **script is correct**. The error occurs because:

### **Reason 1: Only Running Part of the Script** ??
If you're only executing the FNOL section (Section 3) without executing the Policies table creation (Section 2) first, SQL Server can't find the Policies table.

**Fix**: Execute the **entire script** from top to bottom, NOT just individual sections.

### **Reason 2: Database Already Exists with Old Schema** ??
If you previously tried to execute the script and it failed partway through, remnants might be interfering.

**Fix**: Drop and recreate the database (see below).

### **Reason 3: Case Sensitivity or Table Name Mismatch** ??
Very unlikely, but the table name must be exactly `Policies` (with capital P).

**Fix**: Verify the Policies table exists with exact name.

---

## ?? **VERIFICATION STEPS**

### **Step 1: Check if Policies Table Exists**

```sql
USE ClaimsPortal;
GO

SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Policies';
```

**Expected Result**: Should return one row with `Policies`

**If empty**: Policies table doesn't exist ? Policies creation failed

---

### **Step 2: Check Policies Table Structure**

```sql
SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Policies' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;
```

**Expected Result**: Should show columns including PolicyNumber

**If empty**: Policies table doesn't exist

---

### **Step 3: Check for UNIQUE Constraint on PolicyNumber**

```sql
SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE 
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Policies' AND TABLE_SCHEMA = 'dbo';
```

**Expected Result**: Should show constraints

---

## ??? **FIX OPTIONS**

### **Option A: Drop and Recreate Database** ? RECOMMENDED

**Step 1**: Drop the database
```sql
USE master;
GO

DROP DATABASE IF EXISTS ClaimsPortal;
GO
```

**Step 2**: Recreate the database
```sql
CREATE DATABASE ClaimsPortal;
GO
```

**Step 3**: Execute the **ENTIRE** script from beginning to end
```sql
USE ClaimsPortal;
GO

-- Paste entire 001_InitialSchema.sql here
```

---

### **Option B: Drop Just the Problem Tables**

If you want to keep the database:

```sql
USE ClaimsPortal;
GO

-- Drop dependent tables first
DROP TABLE IF EXISTS [dbo].[Claimants];
DROP TABLE IF EXISTS [dbo].[SubClaims];
DROP TABLE IF EXISTS [dbo].[Vehicles];
DROP TABLE IF EXISTS [dbo].[FNOL];
DROP TABLE IF EXISTS [dbo].[Policies];

-- Then re-execute SECTION 2 and 3 of the script
```

---

### **Option C: Execute Entire Script in One Go** ? BEST PRACTICE

1. **Don't select individual sections**
2. **Copy the ENTIRE 001_InitialSchema.sql file**
3. **Paste it all into SSMS**
4. **Execute** (F5) - it will run all sections in order

---

## ? **CORRECT EXECUTION STEPS**

### **In SQL Server Management Studio:**

```
1. File ? Open ? Database\001_InitialSchema.sql
2. Make sure ClaimsPortal database is selected
3. Press Ctrl+A (Select All) - THIS IS IMPORTANT!
4. Press F5 (Execute)
5. Wait for completion
```

**NOT this (wrong)**:
- Don't try to highlight just Section 3 (FNOL)
- Don't try to skip sections
- Don't close and reopen between sections

**DO this (correct)**:
- Execute the entire file all at once
- From top to bottom
- All sections in sequence

---

## ?? **Why This Works**

The script has proper `GO` batch separators:

```sql
-- SECTION 2: Creates Policies table
CREATE TABLE Policies (...)
GO

-- SECTION 3: Creates FNOL table (references Policies)
CREATE TABLE FNOL (
    ...
    CONSTRAINT FK_FNOL_Policy FOREIGN KEY (PolicyNumber) REFERENCES Policies(PolicyNumber),
    ...
)
```

When executed in order with `GO` separators:
1. **Policies table is created first** ?
2. **Then FNOL table references it** ?
3. **Foreign key constraint works** ?

---

## ?? **CHECKLIST**

Before executing the script:

- [ ] Database `ClaimsPortal` exists
- [ ] You're connecting to the right server: `HICD09062024\SQLEXPRESS`
- [ ] You're using the correct database: `ClaimsPortal`
- [ ] You have the file: `Database/001_InitialSchema.sql`
- [ ] You're selecting **ALL** of the file (Ctrl+A)
- [ ] You're NOT selecting just one section

---

## ?? **RECOMMENDED APPROACH**

```
1. Drop the problematic ClaimsPortal database
2. Create a fresh ClaimsPortal database
3. Execute the ENTIRE 001_InitialSchema.sql script
4. Done!
```

**Time**: 5 minutes

---

## ? **VERIFICATION AFTER FIX**

Once the script completes successfully, verify:

```sql
USE ClaimsPortal;
GO

-- Should return 9
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- Should return 1
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Policies';

-- Should return 1
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FNOL';
```

---

**Bottom Line**: The script is correct. The issue is likely that you're:
1. Not executing the complete script
2. Executing sections out of order
3. Running against a corrupted database

Try **Option A** (drop and recreate) - it will definitely fix the issue!
