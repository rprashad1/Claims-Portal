# ClaimsPortal Version Information

## Current Version: 1.2 - VendorMaster Separation

**Release Date:** January 5, 2026
**Backup Locations:** 
- `C:\Projects\Claims\ClaimsPortal_v1.0_Stable` (Base stable version)
- `C:\Projects\Claims\ClaimsPortal_v1.1_VendorSearch` (Vendor search feature)

---

## What's New in Version 1.2

### ? Separate VendorMaster Table
- **VendorMaster Table** - Dedicated table for vendors (50K records)
- **VendorAddress Table** - Vendor addresses linked to VendorMaster
- **Performance Optimization** - Vendor searches now scan 50K instead of 500K+ records
- **Future-Proof Architecture** - Vendors grow slowly (~500/year) vs claim entities (~50K/year)

### Database Changes:
- New `VendorMaster` table with optimized indexes
- New `VendorAddress` table for vendor addresses
- Added `HospitalVendorId` and `AttorneyVendorId` to Claimants
- Added `VendorId` to FnolAuthorities
- Migration script migrates existing vendors from EntityMaster

### New Files:
- `Database\019_Create_VendorMaster_Table.sql`
- `VENDORMASTER_ARCHITECTURE.md`

---

## What's in Version 1.1 (Vendor Search)

### ? Vendor Search & Duplicate Detection
- **Hospital Search** - Search hospitals from Vendor Master in injury forms
- **Attorney Search** - Search attorneys from Vendor Master
- **Authority Search** - Search police/fire departments from Vendor Master
- **Duplicate Detection** - Warns when manually entering vendors that already exist

---

## What's Working in This Version

### ? FNOL Entry (5 Steps)
- Step 1: Loss Details (Date, Time, Location, Witnesses, Authorities)
- Step 2: Policy & Insured (Policy Search, Vehicle Selection, Insured Info)
- Step 3: Driver & Injury (Driver Info, Injury Details, Attorney, Passengers)
- Step 4: Third Parties (Vehicles, Pedestrians, Bicyclists, Property Damage)
- Step 5: Review & Save (Complete Summary, Sub-Claims Generation)

### ? Vendor Search Features
- Hospital search with duplicate detection
- Attorney search (defense & plaintiff)
- Authority search (police & fire departments)
- Auto-population from Vendor Master
- Duplicate warning on manual entry

### ? Claim Detail View (8 Tabs)
- Loss Details Tab
- Insured & Vehicle Tab
- Driver & Passengers Tab
- Third Parties Tab
- Sub-Claims Summary Tab
- Financials Tab
- Claims Log Tab
- Communication Tab

### ? Sub-Claims Management
- Close/Reopen functionality
- Reserve management
- Audit trail

### ? Vendor Master
- Full CRUD operations
- Address management
- Payment settings

---

## Database Scripts Applied

### Core Schema (Run in order)
1. `000_CreateLiveNewDatabase.sql`
2. `001_InitialSchema.sql`
3. `002_VendorMaster_SchemaUpdates.sql`
4. `003_Vehicles_SchemaUpdates.sql`
5. `004_Fix_ClaimNumber_UniqueConstraint.sql`
6. `005_Add_PolicyDetails_ToFNOL.sql`
7. `006_SubClaim_Status_Fields.sql`
8. `007_EntityMaster_Claimant_Architecture.sql`
9. `008_SubClaim_ClaimantEntityId.sql`
10. `009_Vehicle_Owner_And_CoverageLimits_Changes.sql`
11. `010_ThirdParty_Insurance_Fields.sql`
12. `011_CloseReopen_SubClaims_AuditTrail.sql`
13. `012_CleanupAndFixes.sql`
14. `013_Add_Insured_Details_ToFNOL.sql`
15. `014_Add_PropertyDamages_Table.sql`
16. `015_Add_Sample_Adjusters.sql`
17. `016_Add_Hospital_Info_To_Claimants.sql`
18. `017_Add_InjuredParty_To_Claimants.sql`
19. `018_Add_Sample_Vendors.sql` *(Optional - for testing)*
20. `019_Create_VendorMaster_Table.sql` *(NEW - VendorMaster separation)*

---

## Data Volume Expectations

| Table | Current | 5-Year Projection | Notes |
|-------|---------|-------------------|-------|
| VendorMaster | 50,000 | 52,500 | Slow growth (~500/year) |
| EntityMaster | 500,000 | 750,000+ | Fast growth (~50K/year) |
| Claimants | 320,000 | 470,000 | 30K/year |
| Vehicles | 155,000 | 230,000 | Third party vehicles |
| Witnesses | 25,000 | 50,000 | 5K/year |

---

## How to Restore Previous Versions

### Restore to v1.0 (Base Stable)
```powershell
robocopy "C:\Projects\Claims\ClaimsPortal_v1.0_Stable" "C:\Projects\Claims\ClaimsPortal" /E /XD bin obj .vs
```

### Restore to v1.1 (Before VendorMaster Separation)
```powershell
robocopy "C:\Projects\Claims\ClaimsPortal_v1.1_VendorSearch" "C:\Projects\Claims\ClaimsPortal" /E /XD bin obj .vs
