# ? **DATABASE INTEGRATION COMPLETE**

## ?? **What You've Done**

? Created ClaimsPortal database with 9 tables
? Installed Entity Framework Core (EF Core 10.0.1)
? Created `ClaimsPortalDbContext` with all entity models
? Configured connection string in `appsettings.json`
? Registered DbContext in `Program.cs`
? Build successful - **No errors!**

---

## ?? **NEXT STEPS (What To Do Now)**

### **STEP 1: Test Database Connection** (5 minutes)

Run the application and verify it starts without errors:

```
1. Press F5 (Run application)
2. Wait for Blazor to load
3. Navigate to any page
4. If no database errors ? ? Connected!
```

**Expected**: App loads successfully, no database connection errors.

---

### **STEP 2: Create a Data Service to Test Database** (10 minutes)

Create `Services/LookupService.cs` to test database access:

```csharp
using ClaimsPortal.Data;

namespace ClaimsPortal.Services
{
    public interface ILookupService
    {
        Task<List<LookupCode>> GetLookupCodesAsync(string recordType);
    }

    public class LookupService : ILookupService
    {
        private readonly ClaimsPortalDbContext _context;

        public LookupService(ClaimsPortalDbContext context)
        {
            _context = context;
        }

        public async Task<List<LookupCode>> GetLookupCodesAsync(string recordType)
        {
            return await _context.LookupCodes
                .Where(l => l.RecordType == recordType && l.RecordStatus == 'Y')
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
        }
    }
}
```

---

### **STEP 3: Update Program.cs to Use Real Service** (2 minutes)

Replace the Mock with the real service:

```csharp
// OLD - Comment out:
// builder.Services.AddScoped<ILookupService, MockLookupService>();

// NEW - Add:
builder.Services.AddScoped<ILookupService, LookupService>();
```

---

### **STEP 4: Update ClaimService to Use Database** (15 minutes)

Modify `Services/ClaimService.cs` to:
- Accept `ClaimsPortalDbContext` in constructor
- Load lookup codes from database instead of mock data
- Save claims to database

**Key changes**:
```csharp
public class ClaimService : IClaimService
{
    private readonly ClaimsPortalDbContext _context;
    private readonly ILookupService _lookupService;

    public ClaimService(ClaimsPortalDbContext context, ILookupService lookupService)
    {
        _context = context;
        _lookupService = lookupService;
    }

    // Now use _context.Policies, _context.FNOLs, etc.
}
```

---

### **STEP 5: Update PolicyService for Database** (10 minutes)

Modify `Services/PolicyService.cs` to load from database:

```csharp
public async Task<Policy> GetPolicyAsync(string policyNumber)
{
    return await _context.Policies
        .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
}
```

---

### **STEP 6: Test FNOL Creation with Real Data** (10 minutes)

Test the FNOL wizard:
1. Start the app (F5)
2. Create a new FNOL
3. Select a policy (loads from database)
4. Submit the form
5. Verify data is saved to ClaimsPortal database

**Check**:
- Open SQL Server Management Studio
- Query the FNOL table: `SELECT * FROM ClaimsPortal.dbo.FNOL`
- Should see new record!

---

## ?? **PRIORITY ORDER**

| Priority | Task | Time | Status |
|----------|------|------|--------|
| ?? **1** | Test database connection | 5 min | Start HERE |
| ?? **2** | Create LookupService | 10 min | After step 1 |
| ?? **3** | Update Program.cs | 2 min | After step 2 |
| ?? **4** | Update ClaimService | 15 min | After step 3 |
| ?? **5** | Update PolicyService | 10 min | After step 4 |
| ?? **6** | Test FNOL creation | 10 min | After step 5 |

---

## ?? **Quick Reference: EF Core Methods**

```csharp
// Query
var policies = await _context.Policies.ToListAsync();
var policy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyNumber == "POL-123");

// Add
_context.Fnols.Add(newFnol);
await _context.SaveChangesAsync();

// Update
policy.PolicyStatus = 'C';
await _context.SaveChangesAsync();

// Delete
_context.Fnols.Remove(fnol);
await _context.SaveChangesAsync();

// Include related data
var fnol = await _context.FNOLs
    .Include(f => f.Vehicles)
    .FirstOrDefaultAsync(f => f.FnolId == 1);
```

---

## ? **Verification Checklist**

- [ ] Application runs without database errors (Step 1)
- [ ] Can query lookup codes from database (Step 2)
- [ ] LookupService registered in Program.cs (Step 3)
- [ ] ClaimService uses database instead of mock (Step 4)
- [ ] PolicyService loads from database (Step 5)
- [ ] New FNOL is saved to database (Step 6)
- [ ] Can query FNOL data in SQL Server

---

## ?? **Important Notes**

**Database is real** - Changes persist in SQL Server
**Mocks still exist** - They're used by other services (Address, Adjuster)
**Gradual migration** - You can replace one service at a time
**No migrations needed** - Schema already created

---

## ?? **Success Criteria**

? App runs F5 without errors
? Database connection works
? Can create FNOL and see it in SQL Server
? Lookup codes load from database
? No mock data in actual flows

---

**You're ready to start integrating!** ??

Begin with **STEP 1: Test Database Connection** - it should take 5 minutes.

Report back when the application runs successfully!
