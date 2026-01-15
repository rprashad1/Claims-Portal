# ? **QUICK FIX - 3 MINUTES**

## ?? **Do This Right Now**

### **Step 1: Drop the problematic database**

```sql
USE master;
GO

DROP DATABASE IF EXISTS ClaimsPortal;
GO
```

Execute this in SSMS.

---

### **Step 2: Create fresh database**

```sql
CREATE DATABASE ClaimsPortal;
GO
```

Execute this.

---

### **Step 3: Open the entire script**

1. File ? Open ? `Database/001_InitialSchema.sql`
2. **Ctrl+A** (Select ALL - this is important!)
3. **F5** (Execute)
4. Wait for completion (2-3 minutes)

---

## ? **Expected Result**

```
Command(s) completed successfully.
```

**No errors.**

---

## ?? **Verify It Worked**

```sql
USE ClaimsPortal;
GO

SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
-- Should return: 9
```

---

**That's it!** ??

The Foreign Key error is gone because you're executing the complete script with all dependencies in the correct order.
