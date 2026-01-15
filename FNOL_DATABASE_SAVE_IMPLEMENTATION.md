# FNOL Database Save Implementation

## ? Implementation Complete

This document describes how FNOL form data is saved to the database.

---

## ?? Data Mapping Summary

### Party Types & Database Tables

| Screen | Party | Database Table | EntityGroupCode / VehicleParty |
|--------|-------|----------------|-------------------------------|
| Step 2 | Insured Vehicle | `Vehicles` | VehicleParty = `IPV` |
| Step 3 | Insured Driver | `Claimants` + `EntityMaster` | ClaimantType = `InsuredDriver` |
| Step 3 | Insured Passenger | `Claimants` + `EntityMaster` | ClaimantType = `InsuredPassenger` |
| Step 4 | Third Party Owner | `EntityMaster` | PartyType = `ThirdPartyOwner` |
| Step 4 | Third Party Vehicle | `Vehicles` | VehicleParty = `TPV` |
| Step 4 | Third Party Driver | `Claimants` + `EntityMaster` | ClaimantType = `ThirdPartyDriver` |
| Step 4 | Third Party Passenger | `Claimants` + `EntityMaster` | ClaimantType = `ThirdPartyPassenger` |
| Step 4 | Property Owner | `Claimants` + `EntityMaster` | ClaimantType = `PropertyOwner` |
| Step 1 | Witness | `EntityMaster` + `FnolWitnesses` | EntityGroupCode = `Witness` |
| Step 1 | Authority (Police/Fire) | `EntityMaster` + `FnolAuthorities` | EntityGroupCode = `Vendor` |
| Step 1 | Reported By (Other) | `EntityMaster` | EntityGroupCode = `Vendor` |
| All | Attorney | `EntityMaster` | VendorType = `Attorney` |

---

## ??? Database Tables Used

### 1. FNOL Table (Header)
Stores the main FNOL record with loss details.

| Column | Source |
|--------|--------|
| FnolNumber | Auto-generated |
| PolicyNumber | Step 2 - Policy |
| DateOfLoss | Step 1 - Loss Details |
| TimeOfLoss | Step 1 - Loss Details |
| ReportDate | Step 1 - Loss Details |
| LossLocation | Step 1 - Loss Details |
| LossLocation2 | Step 1 - Loss Details |
| CauseOfLoss | Step 1 - Loss Details |
| WeatherConditions | Step 1 - Loss Details |
| LossDescription | Step 1 - Loss Details |
| HasVehicleDamage | Step 2 - Vehicle |
| HasInjury | Step 1 - Loss Details |
| HasPropertyDamage | Step 1 - Loss Details |
| HasOtherVehiclesInvolved | Step 1 - Loss Details |
| ReportedBy | Step 1 - Loss Details |
| ReportedByName | Step 1 - Loss Details |
| ReportedByPhone | Step 1 - Loss Details |
| ReportedByEmail | Step 1 - Loss Details |
| ReportingMethod | Step 1 - Loss Details |

### 2. Vehicles Table
Stores vehicle information for insured and third party vehicles.

| Column | Source |
|--------|--------|
| VehicleParty | `IPV` (Insured) or `TPV` (Third Party) |
| VIN | Step 2 / Step 4 |
| Make, Model, Year | Step 2 / Step 4 |
| PlateNumber, PlateState | Step 2 / Step 4 |
| IsVehicleDamaged | Step 2 / Step 4 |
| HasDashCam | Step 2 |
| DidVehicleRollOver | Step 2 |
| WasTowed, IsInStorage | Step 2 |
| DamageDetails | Step 2 |

### 3. EntityMaster Table
Stores all parties (people and organizations).

| EntityGroupCode | Used For |
|-----------------|----------|
| `Claimant` | Drivers, Passengers, Property Owners |
| `Vendor` | Attorneys, Authorities, Reporters |
| `Witness` | Witnesses |

### 4. AddressMaster Table
Stores addresses linked to EntityMaster.

### 5. Claimants Table
Links claimants to FNOLs with injury information.

| ClaimantType | Description |
|--------------|-------------|
| `InsuredDriver` | Driver of insured vehicle |
| `InsuredPassenger` | Passenger in insured vehicle |
| `ThirdPartyOwner` | Owner of third party |
| `ThirdPartyDriver` | Driver of third party vehicle |
| `ThirdPartyPassenger` | Passenger in third party vehicle |
| `ThirdPartyPedestrian` | Pedestrian third party |
| `ThirdPartyBicyclist` | Bicyclist third party |
| `PropertyOwner` | Owner of damaged property |

### 6. SubClaims Table
Stores features/sub-claims with coverage and reserves.

### 7. FnolWitnesses Table (New)
Links witnesses to FNOL records.

### 8. FnolAuthorities Table (New)
Links authorities (Police/Fire) to FNOL records.

---

## ?? Save Flow

```
User clicks "Save & Submit" on Step 5
                ?
        SubmitClaim() called
                ?
      CollectAllStepData()
                ?
   SaveCompleteFnolAsync(request)
                ?
    ?????????????????????????
    ?  BEGIN TRANSACTION    ?
    ?????????????????????????
                ?
    1. Create FNOL record
                ?
    2. Save Reported By (if Other) ? EntityMaster
                ?
    3. Save Witnesses ? EntityMaster + FnolWitnesses
                ?
    4. Save Authorities ? EntityMaster + FnolAuthorities
                ?
    5. Save Insured Vehicle ? Vehicles (VehicleParty = 'IPV')
                ?
    6. Save Insured Driver ? EntityMaster + Claimants
                ?
    7. Save Insured Passengers ? EntityMaster + Claimants
                ?
    8. Save Third Parties:
       - Owner ? EntityMaster
       - Vehicle ? Vehicles (VehicleParty = 'TPV')
       - Driver ? EntityMaster + Claimants
                ?
    9. Save Property Damages ? EntityMaster + Claimants
                ?
    10. Save Sub-Claims/Features ? SubClaims
                ?
    ?????????????????????????
    ?   COMMIT TRANSACTION  ?
    ?????????????????????????
                ?
        Success Modal shown
```

---

## ?? Files Modified

| File | Changes |
|------|---------|
| `Data/ClaimsPortalDbContext.cs` | Added FnolWitness, FnolAuthority entities; Added ReportedBy fields to Fnol |
| `Services/DatabaseClaimService.cs` | Added SaveCompleteFnolAsync method with all data saving logic |
| `Components/Pages/Fnol/FnolNew.razor` | Updated SubmitClaim to use SaveCompleteFnolAsync |
| `Database/003_Vehicles_SchemaUpdates.sql` | Added new columns and tables |

---

## ??? SQL Script to Run

Execute `Database/003_Vehicles_SchemaUpdates.sql` to add:

### New Columns:
- `FNOL.LossLocation2`
- `FNOL.HasOtherVehiclesInvolved`
- `FNOL.ReportedBy`
- `FNOL.ReportedByName`
- `FNOL.ReportedByPhone`
- `FNOL.ReportedByEmail`
- `FNOL.ReportingMethod`
- `FNOL.ReportedByEntityId`
- `Vehicles.HasDashCam`
- `Vehicles.DidVehicleRollOver`
- `Vehicles.PlateState`

### New Tables:
- `FnolWitnesses` - Links witnesses to FNOL
- `FnolAuthorities` - Links authorities to FNOL

---

## ? Build Status

```
Build: SUCCESSFUL
Errors: 0
Warnings: 0
```

---

## ?? Testing Checklist

- [ ] Create new FNOL with all steps filled
- [ ] Verify FNOL record created in database
- [ ] Verify Insured Vehicle saved with VehicleParty = 'IPV'
- [ ] Verify Third Party Vehicle saved with VehicleParty = 'TPV'
- [ ] Verify Witnesses saved in EntityMaster + FnolWitnesses
- [ ] Verify Authorities saved in EntityMaster + FnolAuthorities
- [ ] Verify Claimants saved with correct ClaimantType
- [ ] Verify Attorneys saved in EntityMaster with VendorType = 'Attorney'
- [ ] Verify Sub-Claims saved with coverage and reserves
- [ ] Verify transaction rollback on error

---

**Implementation Date:** Today
**Status:** ? Complete and Ready for Testing
