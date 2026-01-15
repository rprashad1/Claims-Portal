# ?? VENDOR MASTER MODULE - QUICK START GUIDE

## ?? Where to Find It

**URL**: `/vendor-master`
**Menu**: Administration ? Vendor Master
**Route**: Component path `Components/Pages/VendorMaster.razor`

---

## ?? Quick Start (5 Minutes)

### Step 1: Search Vendors
```
1. Go to Vendor Master page
2. Enter vendor name or FEIN in search box
3. (Optional) Filter by type or status
4. Click "Search"
5. Results appear in table below
```

### Step 2: Add New Vendor
```
1. Click "Add New Vendor" button (top right)
2. Modal opens with vendor form
3. Enter required fields:
   - Vendor Type (dropdown)
   - Entity Type (Individual/Business)
   - Name (or Company Name)
   - FEIN #
   - Effective Date
4. Add addresses (at least 1 main address required)
5. Fill contact info
6. Configure payment preferences (optional)
7. Click "Save Vendor"
```

### Step 3: Edit Vendor
```
1. Find vendor in search results
2. Click vendor row or Edit button (pencil icon)
3. Modify any fields
4. Update addresses as needed
5. Click "Save Vendor"
```

### Step 4: Disable Vendor
```
1. Find active vendor in results
2. Click red "Disable" button (ban icon)
3. Confirm in dialog
4. Status changes to "Disabled"
```

### Step 5: Enable Vendor
```
1. Search for vendor (include Disabled status)
2. Find disabled vendor
3. Click "Enable" button (green checkmark)
4. Confirm in dialog
5. Status changes back to "Active"
```

---

## ?? Search Operators

### By Name
```
"Smith Law" ? Finds "Smith Law Firm"
"Medical" ? Finds "Medical Clinic"
```

### By FEIN
```
"12-3456789" ? Exact FEIN match
```

### Filters
```
Vendor Type: Medical Provider, Hospital, Attorney, etc.
Status: Active or Disabled
```

---

## ?? Required Fields

### Basic Information
- ? Vendor Type (dropdown)
- ? Entity Type (Individual/Business)
- ? Vendor Name
- ? FEIN # (must be unique)
- ? Effective Date

### Address
- ? Street Address
- ? City
- ? State
- ? ZIP Code
- ? Address Type (Main required)

### Contact
- ? Contact Name
- Phone/Email (optional but recommended)

---

## ?? UI Components

### Main Page
```
???????????????????????????????????????
?  Vendor Master                      ?
?                   [Add New Vendor]  ?
???????????????????????????????????????
? Search Section                      ?
? [Search Term] [Type v] [Status v]   ?
? [Search] [Clear]                    ?
???????????????????????????????????????
? Results Table                       ?
? Name ? Type ? FEIN ? Phone ? Status ?
? ...                                 ?
???????????????????????????????????????
```

### Add/Edit Modal
```
??????????????????????????????????????
? Add New Vendor              [X]    ?
??????????????????????????????????????
? Basic Information                  ?
? [Vendor Type] [Entity Type]       ?
? [Name] [DBA - if Business]        ?
? [FEIN] [Effective Date] [Term]    ?
?                                    ?
? Tax & Compliance                   ?
? [?] W-9 Received                  ?
? [?] Subject to 1099               ?
? [?] Backup Withholding            ?
?                                    ?
? Contact Information                ?
? [Name] [Email]                    ?
? [Phone] [Mobile] [Fax]            ?
?                                    ?
? Addresses                          ?
? [+ Add Address]                   ?
? ????????????????????????????????   ?
? ? Main Address        [Edit][X] ?   ?
? ? 123 Main St                  ?   ?
? ? City, ST 12345               ?   ?
? ????????????????????????????????   ?
?                                    ?
? Payment Information                ?
? [?] Receives Bulk Payments       ?
? [Frequency] [Date/Day]           ?
??????????????????????????????????????
? [Cancel]           [Save Vendor]   ?
??????????????????????????????????????
```

---

## ?? Key Features

| Feature | Details |
|---------|---------|
| Search | By Name, DBA, FEIN with smart matching |
| Addresses | Multiple addresses, exactly 1 main |
| Contact Info | Person name, phones, email, fax |
| Vendor Types | 9 different types (Medical, Hospital, etc.) |
| Entity Type | Individual or Business |
| Tax Info | W-9, 1099, Backup Withholding tracking |
| Payment | Monthly (dates 1-31) or Weekly (Mon-Fri) |
| Status | Active or Disabled (soft delete) |

---

## ? Validation Checklist

Before saving a vendor, ensure:
- [ ] Vendor Name is entered
- [ ] FEIN # is unique and properly formatted
- [ ] Effective Date is set
- [ ] If Business: Doing Business As (DBA) is filled
- [ ] At least one Main address exists
- [ ] Main address is complete (Street, City, State, ZIP)
- [ ] Contact Name is entered
- [ ] If Bulk Payments: Frequency and Date/Day are set

---

## ?? Tips & Tricks

### Search Tips
```
Use partial names for smart search
Search by FEIN for exact vendor lookup
Filter by type to narrow results
Include status filter to find disabled vendors
```

### Address Management
```
Can have multiple addresses per vendor
Only one Main address allowed
Change address type instead of deleting
Disable addresses instead of removing
```

### Payment Setup
```
Monthly: Select date 1-31 (or Last day)
Weekly: Select Monday-Friday
If switching between Monthly/Weekly, 
old date/day clears automatically
```

---

## ?? Common Issues

### "FEIN already exists"
- FEIN must be unique per vendor
- Check if vendor with this FEIN already in system
- Suggestion: Update existing vendor instead

### "Can only have one main address"
- Each vendor needs exactly one Main address
- Change other addresses to Temporary or Alternate
- Delete extra main address first, then add new one

### "Main address must be complete"
- All address fields required for main address:
  - Street Address
  - City
  - State
  - ZIP Code

### Addresses not displaying
- Ensure address has been saved
- Check address type is set correctly
- Verify address status is Active

---

## ?? Field Reference

### Vendor Types
```
Medical Provider      - Doctors, clinics
Hospital             - Healthcare facility
Defense Attorney     - Defense legal counsel
Plaintiff Attorney   - Plaintiff legal counsel
Towing Service       - Towing companies
Repair Shop          - Auto repair
Rental Car Company   - Vehicle rental
Insurance Carrier    - Other insurance co.
Other               - Miscellaneous
```

### Address Types
```
Main              - Primary vendor address
Temporary Address - Temporary location
Alternate Address - Backup/secondary address
```

### Status Options
```
Active   - Vendor is available for use
Disabled - Vendor is inactive (can be re-enabled)
```

### Payment Frequencies
```
Monthly - Payment on specific date (1-31)
Weekly  - Payment on specific day (Mon-Fri)
```

---

## ?? Support

### For Database Integration
See: VENDOR_MASTER_IMPLEMENTATION_GUIDE.md

### For API Usage
See: Services/VendorService.cs interface

### For UI Customization
See: Components/Pages/VendorMaster.razor
See: Components/Modals/VendorDetailModal.razor

---

## ?? Next Steps

1. **Try the module**: Add a test vendor
2. **Practice search**: Try different search terms
3. **Test addresses**: Add multiple addresses
4. **Configure payments**: Set up payment schedule
5. **Disable/Enable**: Test disabling a vendor

---

**Module**: Vendor Master
**Status**: ? Production Ready
**Last Updated**: Today
