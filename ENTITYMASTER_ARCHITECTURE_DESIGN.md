# EntityMaster Architecture - Implementation Guide

## Executive Summary

Your Claims Portal **already has the EntityMaster pattern implemented** in the database layer. This guide explains how the `DatabaseClaimService` now properly uses it to:

1. **Central Claimant Tracking** - All claimants linked via Claimants table with `ClaimantEntityId`
2. **Address Linking** - All addresses linked to EntityMaster via AddressMaster
3. **SubClaim Linking** - SubClaims now linked to EntityMaster via `ClaimantEntityId`

---

## Implementation Complete ?

The following has been implemented:

### Database Changes (Run Migration Script)
- `Database/008_SubClaim_ClaimantEntityId.sql`
  - Adds `ClaimantEntityId` to SubClaims table
  - Adds `AssignedAdjusterName` to SubClaims table
  - Adds `ClosedBy`, `ReopenedDate`, `ReopenedBy` to SubClaims table
  - Creates `vw_ClaimantFullDetails` view
  - Creates `vw_SubClaimWithEntity` view
  - Creates `sp_CreateClaimantWithEntity` stored procedure
  - Creates index on `ClaimantEntityId`

### Code Changes

#### Data Model (`Data/ClaimsPortalDbContext.cs`)
- Added `ClaimantEntityId` to `SubClaim` class
- Added `AssignedAdjusterName` to `SubClaim` class
- Added `ClosedBy`, `ReopenedDate`, `ReopenedBy` to `SubClaim` class

#### Service Layer (`Services/DatabaseClaimService.cs`)
- Updated `SaveCompleteFnolAsync` to:
  - Create EntityMaster records for all parties
  - Create AddressMaster records linked to EntityMaster
  - Create Claimant records with `ClaimantEntityId` FK
  - Create SubClaim records with `ClaimantEntityId` FK
  - Track EntityIds in dictionary for SubClaim linking

- Added new methods:
  - `GetClaimantsForClaimAsync(string claimNumber)` - Get all claimants with entity/address details
  - `GetClaimantsForClaimAsync(long fnolId)` - Get all claimants by FNOL ID
  - `GetClaimantWithDetailsAsync(long claimantId)` - Get single claimant with full details
  - `GetSubClaimsForClaimAsync(long fnolId)` - Get all sub-claims for a claim

- Added new DTOs:
  - `ClaimantWithDetails` - Full claimant info with entity, address, attorney, and sub-claims
  - `SubClaimSummary` - Summary of sub-claim for claimant display

---

## Data Flow

When saving an FNOL, the flow is now:

```
???????????????????????????????????????????????????????????????????????
?                     FNOL SAVE FLOW                                   ?
???????????????????????????????????????????????????????????????????????
?                                                                     ?
?  1. Save FNOL Record                                                ?
?     ??? FNOL table (get FnolId)                                     ?
?                                                                     ?
?  2. For each Person (Driver, Passenger, ThirdParty, PropertyOwner): ?
?     ??? Create EntityMaster ? Get EntityId                          ?
?     ??? Create AddressMaster ? Link via EntityId                    ?
?     ??? Create Claimant ? Link via ClaimantEntityId                 ?
?     ??? Store EntityId in claimantEntityMap[name]                   ?
?                                                                     ?
?  3. For each SubClaim:                                              ?
?     ??? Look up EntityId from claimantEntityMap[claimantName]       ?
?     ??? Create SubClaim ? Link via ClaimantEntityId                 ?
?                                                                     ?
???????????????????????????????????????????????????????????????????????
```

---

## Current Database Architecture

```
???????????????????????????????????????????????????????????????????????
?                    DATABASE RELATIONSHIPS                            ?
???????????????????????????????????????????????????????????????????????
?                                                                     ?
?                      ????????????????????                           ?
?                      ?   EntityMaster   ?                           ?
?                      ????????????????????                           ?
?                      ? EntityId (PK)    ?                           ?
?                      ? EntityType (B/I) ?                           ?
?                      ? PartyType        ?                           ?
?                      ? EntityName       ?                           ?
?                      ? Phone, Email     ?                           ?
?                      ? FEINorSS         ?                           ?
?                      ? LicenseNumber    ?                           ?
?                      ????????????????????                           ?
?                               ?                                     ?
?          ???????????????????????????????????????????                ?
?          ?                    ?                    ?                ?
?          ?                    ?                    ?                ?
?  ?????????????????   ?????????????????   ?????????????????         ?
?  ? AddressMaster ?   ?   Claimants   ?   ?  SubClaims    ?         ?
?  ?????????????????   ?????????????????   ?????????????????         ?
?  ? AddressId     ?   ? ClaimantId    ?   ? SubClaimId    ?         ?
?  ? EntityId (FK) ?   ? FnolId (FK)   ?   ? FnolId        ?         ?
?  ? AddressType   ?   ?ClaimantEntityId?  ?ClaimantEntityId? ? NEW  ?
?  ? StreetAddress ?   ? ClaimantName  ?   ? ClaimantName  ?         ?
?  ? City, State   ?   ? ClaimantType  ?   ? Coverage      ?         ?
?  ?????????????????   ? AttorneyEntityId? ? Reserves...   ?         ?
?                      ?????????????????   ?????????????????         ?
?                                                                     ?
???????????????????????????????????????????????????????????????????????
```

---

## Usage Examples

### Get All Claimants for a Claim

```csharp
// Inject the service
@inject IDatabaseClaimService ClaimService

// Get claimants with full details
var claimants = await ClaimService.GetClaimantsForClaimAsync("CLM2025011500001");

foreach (var claimant in claimants)
{
    Console.WriteLine($"Name: {claimant.ClaimantName}");
    Console.WriteLine($"Type: {claimant.ClaimantType}");
    Console.WriteLine($"Phone: {claimant.PhoneNumber}");
    Console.WriteLine($"Address: {claimant.StreetAddress}, {claimant.City}, {claimant.State}");
    
    if (claimant.IsAttorneyRepresented)
    {
        Console.WriteLine($"Attorney: {claimant.AttorneyName} ({claimant.AttorneyFirmName})");
    }
    
    Console.WriteLine($"Sub-Claims: {claimant.SubClaims.Count}");
}
```

### Get SubClaims with Entity Details (SQL)

```sql
-- Using the new view
SELECT 
    FeatureNumber,
    Coverage,
    ClaimantName,
    ClaimantPhone,
    ClaimantEmail,
    StreetAddress,
    City,
    State
FROM vw_SubClaimWithEntity
WHERE ClaimNumber = 'CLM2025011500001';
```

### Get Claimant Full Details (SQL)

```sql
-- Using the new view
SELECT 
    ClaimantName,
    ClaimantType,
    HomeBusinessPhone,
    EntityEmail,
    StreetAddress,
    City,
    State,
    ZipCode,
    HasInjury,
    InjuryType,
    AttorneyName,
    AttorneyFirmName
FROM vw_ClaimantFullDetails
WHERE ClaimNumber = 'CLM2025011500001';
```

---

## PartyType Values

| PartyType | Description |
|-----------|-------------|
| Driver | Insured driver |
| Passenger | Insured vehicle passenger |
| ThirdPartyOwner | Third party vehicle owner |
| ThirdPartyDriver | Third party driver |
| Pedestrian | Pedestrian involved |
| Bicyclist | Bicyclist involved |
| PropertyOwner | Property damage owner |
| Attorney | Legal representative |
| Witness | Witness to accident |
| ReportedBy | Person who reported claim |

---

## Next Steps

### 1. Run Database Migration
```sql
-- Execute this script against your database
Database/008_SubClaim_ClaimantEntityId.sql
```

### 2. Test FNOL Save
Create a new FNOL with all parties and verify:
- EntityMaster records are created
- AddressMaster records are linked
- Claimants have ClaimantEntityId
- SubClaims have ClaimantEntityId

### 3. Query Claimants
Use the new service methods or SQL views to verify data:
```csharp
var claimants = await ClaimService.GetClaimantsForClaimAsync("CLM...");
```

---

## Files Modified

| File | Changes |
|------|---------|
| `Data/ClaimsPortalDbContext.cs` | Added ClaimantEntityId and status fields to SubClaim |
| `Services/DatabaseClaimService.cs` | Updated SaveCompleteFnolAsync, added claimant query methods |
| `Database/008_SubClaim_ClaimantEntityId.sql` | Migration script for new columns and views |
| `ENTITYMASTER_ARCHITECTURE_DESIGN.md` | This documentation |

---

**Document Version**: 2.0  
**Date**: January 2025  
**Status**: Implementation Complete
