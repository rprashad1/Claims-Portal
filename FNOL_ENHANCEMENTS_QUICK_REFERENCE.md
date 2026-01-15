# FNOL Enhancements - Quick Reference Guide

## Overview of Changes

### Step 1: Loss Details - NEW FIELDS

```
?? LOSS DETAILS ??????????????????????????????????
?                                                ?
? Cause of Loss *              [Dropdown]       ?
?   ?? Snow / Wet Road / Red Light            ?
?                                                ?
? Weather Condition *          [Dropdown]       ?
?   ?? Rain / Dense Fog / Slippery Road       ?
?                                                ?
? Loss Description *          [Text Area]       ?
?   ?? Multi-line text box for details        ?
?                                                ?
??????????????????????????????????????????????????
```

---

### Step 2: Policy & Insured - UPDATED FIELDS

```
?? VEHICLE DETAILS ???????????????????????????????
?                                                ?
? ? Vehicle Was Towed                          ?
? ? Dash Cam Installed                         ?
?                                                ?
? ? Vehicle in Storage                         ?
?   ?? Storage Location [Text]     (conditional)?
?                                                ?
?? VEHICLE DAMAGE (if damaged) ??????????????????
?                                                ?
? Damage Details              [Text Area]       ?
?                                                ?
? ? Vehicle Did Roll Over                      ?
? ? Had Water Damage                           ?
? ? Headlights Were On                         ?
? ? Air Bag Deployed                           ?
?                                                ?
?? REMOVED SECTION ???????????????????????????????
? ? Insured Party Involved in Loss             ?
?    (Now captured in Driver & Injury screen)  ?
?                                                ?
??????????????????????????????????????????????????
```

---

### Step 3: Driver & Injury - UPDATED

```
?? DRIVER INFORMATION ????????????????????????????
?                                                ?
? If "Driver is Not Listed on Policy":         ?
?                                                ?
? Driver Name *               [Text]            ?
? License Number             [Text]            ?
? License State              [Text]            ?
? Date of Birth *            [Date] ? NEW      ?
?                                                ?
?? BEHAVIOR CHANGES ?????????????????????????????
?                                                ?
? After "Save Driver & Create Feature":        ?
?   ? Driver info is cleared                   ?
?   ? Driver fields are hidden                 ?
?   ? Ready for next driver entry              ?
?                                                ?
? When "Edit Feature" is clicked:              ?
?   ? Driver form reopens                      ?
?   ? Shows saved data for editing             ?
?                                                ?
??????????????????????????????????????????????????
```

---

### Step 4: Third Parties - NEW FEATURES

```
?? THIRD PARTY VEHICLE DRIVER ????????????????????
?                                                ?
? Driver Name                 [Text]            ?
? License Number             [Text]            ?
? License State              [Text] ? NEW      ?
? Date of Birth              [Date] ? NEW      ?
?                                                ?
?? PROPERTY DAMAGE SECTION (if HasPropertyDamage)
?                                                ?
? [Add Property Damage Button]                 ?
?                                                ?
? ?? PROPERTY DAMAGE GRID ???????????????????  ?
? ? Property Type ? Owner ? Location ? $    ?  ?
? ???????????????????????????????????????????  ?
? ? Building      ? John  ? 123 Main ?5000 ?  ?
? ? Fence         ? Jane  ? 456 Oak  ?2500 ?  ?
? ?               ? [Edit][Delete]         ?  ?
? ???????????????????????????????????????????  ?
?                                                ?
??????????????????????????????????????????????????
```

---

## Model Changes Summary

### ClaimLossDetails (Step 1)
| Field | Type | Notes |
|-------|------|-------|
| CauseOfLoss | string | New: Snow, Wet Road, Red Light |
| LossDescription | string | New: Multi-line description |
| WeatherCondition | string | New: Rain, Dense Fog, Slippery Road |

### VehicleInfo (Step 2)
| Field | Type | Notes |
|-------|------|-------|
| WasTowed | bool | New: Vehicle towing status |
| InStorage | bool | New: Storage status |
| StorageLocation | string? | New: Storage address (conditional) |
| HasDashCam | bool | New: Dash cam availability |
| DamageDetails | string | New: Damage description |
| DidVehicleRollOver | bool | New: Roll over status |
| HadWaterDamage | bool | New: Water damage status |
| AreHeadlightsOn | bool | New: Headlights status |
| DidAirbagDeploy | bool | New: Air bag deployment |

### DriverInfo (Step 3)
| Field | Type | Notes |
|-------|------|-------|
| DateOfBirth | DateTime | New: Driver's birth date |

### PropertyDamage (NEW CLASS)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Unique identifier |
| PropertyType | string | Building, Fence, Other |
| Description | string | Damage description |
| Owner | string | Property owner name |
| OwnerPhone | string | Contact phone |
| OwnerEmail | string | Contact email |
| Location | string | Property address |
| EstimatedDamage | decimal | Damage amount |
| RepairEstimate | string | Repair cost estimate |

### Claim Class
| Field | Type | Notes |
|-------|------|-------|
| PropertyDamages | List<PropertyDamage> | New collection for property damages |

---

## Navigation & Data Flow

### How It Works:
```
???????????????????????????????????????????????
? User navigates between steps                ?
???????????????????????????????????????????????
?                                             ?
? Step 1 (LossDetails)                        ?
?  ?? Collect data                            ?
?     ?? Pass to parent on "Next"             ?
?                                             ?
? Step 2 (PolicyAndInsured)                   ?
?  ?? Collect data                            ?
?     ?? Pass to parent on "Next"             ?
?                                             ?
? Step 3 (DriverAndInjury)                    ?
?  ?? Collect data                            ?
?     ?? Clear form after feature creation    ?
?     ?? Pass to parent on "Next"             ?
?                                             ?
? Step 4 (ThirdParties)                       ?
?  ?? Collect data & property damages         ?
?     ?? Pass to parent on "Next"             ?
?                                             ?
? Step 5 (ReviewAndSave)                      ?
?  ?? Review all data                         ?
?     ?? Submit claim                         ?
?                                             ?
???????????????????????????????????????????????

? Data persists when clicking Previous
? Data cleared only after feature creation in Step 3
```

---

## Validation Rules

### Step 1 (Loss Details)
- **Cause of Loss**: Required
- **Weather Condition**: Required
- **Loss Description**: Required

### Step 2 (Policy & Insured)
- **Policy Number**: Required and must exist
- **Vehicle VIN**: Required
- **All vehicle damage fields**: Optional (visible only if "Is Damaged" = true)

### Step 3 (Driver & Injury)
- **Driver Name**: Required
- **Date of Birth** (if not listed): Optional
- **Injury details**: Required if "Was driver injured" = yes

### Step 4 (Third Parties)
- **Property Damage fields**: Optional (visible only if "HasPropertyDamage" = true)
- **Third party fields**: Optional

---

## Field Display Logic (Conditional Fields)

### Step 1: Loss Details
```csharp
// All fields always visible
```

### Step 2: Vehicle Details
```csharp
// Vehicle Damage section only if InsuredVehicle.IsDamaged == true
if (InsuredVehicle.IsDamaged)
{
    show: Damage Details, Roll Over, Water Damage, Headlights, Air Bag
}

// Storage Location only if InStorage == true
if (InsuredVehicle.InStorage)
{
    show: Storage Location input
}
```

### Step 3: Driver & Injury
```csharp
// Date of Birth only if DriverType == "unlisted"
if (DriverType == "unlisted")
{
    show: Name, License #, License State, Date of Birth
}
```

### Step 4: Third Parties
```csharp
// Property Damage section only if HasPropertyDamage == true
if (HasPropertyDamage)
{
    show: Property Damage Grid and Add button
}
```

---

## Keyboard Shortcuts / Quick Tips

| Action | Shortcut |
|--------|----------|
| Submit form | Enter (after validation) |
| Clear field | Ctrl+A then Delete |
| Navigate back | Previous button |
| Navigate forward | Next button |
| Edit feature | Click pencil icon in grid |
| Delete feature | Click trash icon in grid |

---

## Common Scenarios

### Scenario 1: Vehicle Damage
1. Check "Vehicle is Damaged" in Step 2
2. Vehicle Damage section appears
3. Fill in damage details
4. Proceed to Step 3

### Scenario 2: Property Damage
1. Check "Was there any property damage?" in Step 1
2. Proceed to Step 4
3. Property Damage section appears
4. Add property damage records
5. Proceed to Step 5

### Scenario 3: Third Party Driver
1. Add Third Party in Step 4
2. Fill in License State and Date of Birth
3. Medical treatment date defaults to today
4. Create feature if injured
5. Save and continue

### Scenario 4: Back Navigation
1. At any step, click "Previous"
2. All data is preserved
3. Return to previous step
4. Data is still there for editing

---

## Tips & Best Practices

1. **Use Tab key** to navigate between fields
2. **Required fields** are marked with `*`
3. **Conditional fields** appear based on selections
4. **Dates default to today** in medical treatment fields
5. **Driver form clears** after feature creation - check grid for saved data
6. **Edit features** by clicking the pencil icon
7. **Navigate safely** - Previous button preserves all data

---

## Support & Troubleshooting

### Issue: Driver form cleared
**Solution:** This is normal after creating a feature. Check the "Created Features/Sub-Claims" grid above to see your saved driver.

### Issue: Conditional field not appearing
**Solution:** Make sure the parent checkbox/toggle is checked. Example: Damage Details only appears if "Vehicle is Damaged" is checked.

### Issue: Property Damage section missing
**Solution:** Property Damage section only appears in Step 4 if "Was there any property damage?" was checked in Step 1.

### Issue: Medical date not defaulting
**Solution:** Medical treatment date defaults to today when opening the Third Party modal.

---

## Feature Checklist

- [x] Cause of Loss dropdown in Loss Details
- [x] Weather Condition dropdown in Loss Details
- [x] Loss Description multi-line text in Loss Details
- [x] Vehicle towing flag in Vehicle Details
- [x] Vehicle storage flag and location in Vehicle Details
- [x] Dash cam flag in Vehicle Details
- [x] Vehicle damage details in Vehicle Details
- [x] Vehicle rollover, water damage, headlights, airbag fields
- [x] Removed "Insured Party Involved in Loss" section
- [x] Driver Date of Birth for unlisted drivers
- [x] Driver form reset after feature creation
- [x] Driver form restoration when editing
- [x] Third party driver License State
- [x] Third party driver Date of Birth
- [x] Medical treatment date defaults to today
- [x] Property Damage section (conditional)
- [x] Property Damage grid with CRUD operations
- [x] Data persistence across navigation
- [x] Previous button support

---

**Last Updated:** [Current Date]
**Status:** COMPLETE & IMPLEMENTED
**Build Status:** ? SUCCESSFUL

