# Vendor Search & Duplicate Detection Implementation

## Overview

This implementation adds unified vendor search functionality for **Hospitals, Attorneys, and Authorities** with built-in duplicate detection to maintain data integrity.

## Architecture Decision

### Why EntityMaster (Not a Separate VendorMaster Table)?

After analysis, we decided to **keep EntityMaster as the single source of truth** for all vendors because:

1. **Data Consistency** - All entities (vendors, claimants, witnesses, etc.) are in one table
2. **Relationship Integrity** - Foreign keys from Claimants.AttorneyEntityId, etc. point to EntityMaster
3. **Existing Data** - The Vendor Master UI already saves to EntityMaster
4. **Unified Queries** - Simpler to query all vendor types from one table

Instead of a separate table, we created a **VendorSearchService** that provides efficient, filtered searches of EntityMaster.

---

## Components Created/Modified

### 1. New Service: `VendorSearchService.cs`
**Location:** `Services\VendorSearchService.cs`

Provides unified search across all vendor types:
- `SearchVendorsAsync()` - General vendor search
- `SearchHospitalsAsync()` - Hospital-specific search
- `SearchAttorneysAsync()` - Attorney search (defense & plaintiff)
- `SearchAuthoritiesAsync()` - Authority search (police & fire)
- `CheckDuplicateAsync()` - Duplicate detection

### 2. New Modal: `HospitalSearchModal.razor`
**Location:** `Components\Modals\HospitalSearchModal.razor`

Like AttorneySearchModal but for hospitals:
- Search hospitals from Vendor Master
- Select and auto-populate injury form
- Option to enter manually

### 3. New Model: `HospitalInfo.cs`
**Location:** `Models\HospitalInfo.cs`

Simple model for hospital selection results.

### 4. Updated: `InjuryTemplateUnified.razor`
**Changes:**
- Added "Search Hospital" button
- Integrated HospitalSearchModal
- **Duplicate Detection**: When typing hospital name manually, shows warning if similar hospital exists
- Click on duplicate suggestion to auto-select

### 5. Updated: `AuthorityModal.razor`
**Changes:**
- Now uses VendorSearchService instead of IVendorService
- **Duplicate Detection**: When typing authority name manually, shows warning if similar exists
- Click on duplicate suggestion to auto-select

### 6. Updated: `AttorneySearchModal.razor`
**Changes:**
- Now uses VendorSearchService instead of IVendorService
- Cleaner search results display

### 7. Updated: `Program.cs`
**Changes:**
- Registered `IVendorSearchService` / `VendorSearchService`

---

## Duplicate Detection Flow

When a user types in a vendor name manually (Hospital, Authority, or Attorney):

```
User Types: "Springfield"
    ?
    ??? System waits 500ms (debounce)
    ?
    ??? System searches EntityMaster for matching vendors
    ?
    ??? If matches found:
            ?
            ??? Shows yellow warning box with matches
            ??? User can click a match to auto-select
            ??? Or continue typing to enter new vendor
```

### UI Example (Hospital):
```
????????????????????????????????????????????????????
? Hospital Name                                    ?
? ???????????????????????????????????????????????? ?
? ? Memorial General Hospi...                [??] ? ?
? ???????????????????????????????????????????????? ?
? ?? Similar hospital(s) found in Vendor Master:   ?
?   • Memorial General Hospital - Springfield, IL  ?
?   • Memorial Hospital East - Chicago, IL         ?
?   Click to select, or continue to enter new.     ?
????????????????????????????????????????????????????
```

---

## Database Script

**File:** `Database\018_Add_Sample_Vendors.sql`

Adds sample vendors for testing:
- 5 Hospitals
- 3 Defense Attorneys
- 3 Plaintiff Attorneys
- 4 Police Departments
- 3 Fire Stations

**Run this script to populate test data.**

---

## Vendor Type Constants

The service uses these constants matching EntityMaster.PartyType values:

| Constant | Value |
|----------|-------|
| Hospital | "Hospital" |
| MedicalProvider | "Medical Provider" |
| DefenseAttorney | "Defense Attorney" |
| PlaintiffAttorney | "Plaintiff Attorneys" |
| PoliceDepartment | "Police Department" |
| FireStation | "Fire Station" |

---

## How It Works

### Hospital Search (Injury Form)

1. User checks "Taken to Hospital"
2. User clicks "Search Hospital" button
3. HospitalSearchModal opens
4. User searches and selects hospital
5. Hospital info auto-populates (read-only)
6. User can clear selection to edit manually

### Authority Search (Authority Modal)

1. User selects authority type (Police/Fire)
2. User can search existing authorities
3. If entering manually, duplicate detection shows matches
4. User can click match to auto-select

### Attorney Search (Driver/Third Party Injury)

1. User clicks "Search Attorney"
2. AttorneySearchModal opens
3. User searches and selects
4. Attorney info auto-populates

---

## Benefits

1. **Data Integrity** - Prevents duplicate vendor entries
2. **User Experience** - Easy search and selection
3. **Consistency** - All vendor types use same pattern
4. **Performance** - Debounced searches, efficient queries
5. **Flexibility** - Users can still enter manually if needed

---

## Testing Checklist

- [ ] Run `Database\018_Add_Sample_Vendors.sql`
- [ ] Create FNOL with injured driver
- [ ] Check "Taken to Hospital" - verify Search button appears
- [ ] Search for "Memorial" - verify results show
- [ ] Select a hospital - verify fields populate
- [ ] Add authority in Step 1 - verify search works
- [ ] Type authority name manually - verify duplicate warning shows
- [ ] Add attorney for injured party - verify search works

---

## Future Enhancements

1. **Add to Vendor Master on Save** - When user enters a new vendor manually, optionally add to EntityMaster
2. **Fuzzy Matching** - Use Levenshtein distance for better duplicate detection
3. **Vendor Merge** - Admin tool to merge duplicate vendors
4. **Audit Trail** - Track when vendors are selected vs. manually entered
