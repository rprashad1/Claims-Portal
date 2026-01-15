# Quick Reference - Passenger & Third Party Features

## ?? What Changed

### ? Passenger Workflow (Step 3)
```
Add Passenger ? Enter Details ? Injured? YES ? Enter Injury ? [Save & Create Feature]
    ?
Passenger added to list
Feature modal opens (if injured)
    ?
Complete feature creation
    ?
Feature appears in grid with edit/delete options
```

### ? Third Party Workflow (Step 4)
```
Add Third Party ? Select Type ? Enter Details ? Injured? YES ? Enter Injury ? [Save & Create Feature]
    ?
Third Party added to list
Feature modal opens (if injured)
    ?
Complete feature creation
    ?
Feature appears in grid with edit/delete options
```

---

## ?? What You Get

### Passenger Features
| What | Detail |
|------|--------|
| Injury Capture | Nature, Date, Description |
| Attorney Support | Optional attorney details |
| Feature Creation | Automatic modal for injured |
| Feature Grid | View, Edit, Delete features |
| Data Storage | ClaimType="Passenger" |

### Third Party Features
| What | Detail |
|------|--------|
| Party Types | Vehicle, Pedestrian, Bicyclist, Property, Other |
| Injury Capture | Nature, Date, Description |
| Attorney Support | Optional attorney details |
| Feature Creation | Automatic modal for injured |
| Feature Grid | View, Edit, Delete features |
| Data Storage | ClaimType="ThirdParty" |

---

## ?? Where to Find Things

### PassengerModal.razor
```
Components/Modals/PassengerModal.razor

Key Methods:
- SetInjured(bool) - Toggle injury
- SetHasAttorney(bool) - Toggle attorney
- IsFormValid() - Enhanced validation
- OnAdd() - Save & trigger feature creation
```

### ThirdPartyModal.razor
```
Components/Modals/ThirdPartyModal.razor

Key Methods:
- SetInjured(bool) - Toggle injury
- SetHasAttorney(bool) - Toggle attorney
- IsFormValid() - Enhanced validation
- OnAdd() - Save & trigger feature creation

New Option:
- Bicyclist party type added
```

### FnolStep3_DriverAndInjury.razor
```
Components/Pages/Fnol/FnolStep3_DriverAndInjury.razor

Key Properties:
- CurrentPassengerName - Track passenger for feature
- DriverSubClaims - Store passenger features

Key Methods:
- AddPassengerAndCreateFeature() - NEW
- GetPassengerSubClaims() - NEW
- RemovePassenger() - Updated to clean features
```

### FnolStep4_ThirdParties.razor
```
Components/Pages/Fnol/FnolStep4_ThirdParties.razor

Key Properties:
- ThirdPartySubClaims - NEW
- CurrentThirdPartyName - NEW
- FeatureCounter - NEW

Key Methods:
- AddThirdPartyAndCreateFeature() - NEW
- AddOrUpdateSubClaim() - NEW
- RemoveFeature() - NEW
- RenumberFeatures() - NEW
- GetThirdPartySubClaims() - NEW
```

### FnolNew.razor
```
Components/Pages/Fnol/FnolNew.razor

Key Changes:
- GoToStep4() - Collect driver features only
- GoToStep5() - Collect passenger + third party features
- SubmitClaim() - Submit all features with claim
```

---

## ?? Data Model

```csharp
// All sub-claims stored in one list
Claim.SubClaims
{
    SubClaim
    {
        ClaimType: "Driver"     // or "Passenger" or "ThirdParty"
        ClaimantName: "Name"    // Driver/Passenger/ThirdParty name
        FeatureNumber: "01"     // Auto-numbered
        Coverage: "BI"
        ExpenseReserve: 5000
        IndemnityReserve: 25000
        AssignedAdjusterName: "Raj"
    }
}
```

---

## ? Validation Rules

### For Injured Passenger
```
? Name (required)
? Nature of Injury (required)
? Date of Treatment (required)
? Injury Description (required)
? Attorney Name (required if attorney selected)
? Attorney Firm (required if attorney selected)
```

### For Injured Third Party
```
? Type (required)
? Name (required)
? Nature of Injury (required)
? Injury Description (required)
? Attorney Name (required if attorney selected)
? Attorney Firm (required if attorney selected)
```

---

## ?? User Interactions

### Passenger Modal Buttons
```
[Cancel] [Save & Create Feature]
         ?? Enabled when all required fields complete
```

### Third Party Modal Buttons
```
[Cancel] [Save & Create Feature]
         ?? Enabled when all required fields complete
```

### Feature Grid Actions
```
Feature Grid
?? Edit Button (??) ? Opens SubClaimModal with existing data
?? Delete Button (???) ? Removes feature & renumbers
```

---

## ?? Complete Flow Example

### Scenario: Passenger Injured

```
Step 3: Add Passenger
  1. Click "+ Add Passenger"
  2. Enter: "Jane Smith"
  3. Select: "Yes" for injured
  4. Enter: Whiplash, 2024-01-15, "Neck pain"
  5. Select: "Yes" for attorney
  6. Enter: "John Lawyer", "Smith & Associates"
  7. Click: "[Save & Create Feature]"
  
Feature Modal Opens:
  1. Select: "BI" coverage
  2. Enter: $5,000 expense, $25,000 indemnity
  3. Select: "Raj" as adjuster
  4. Click: "[Create Feature]"
  
Results:
  ? Passenger "Jane Smith" in Passenger List
  ? Feature 02 in Feature Grid
    ?? Coverage: BI - 100/300
    ?? Claimant: Jane Smith
    ?? Reserves: $5,000.00 / $25,000.00
    ?? Adjuster: Raj
    ?? Edit/Delete buttons available
```

### Scenario: Third Party Pedestrian (Injured)

```
Step 4: Add Third Party
  1. Click "+ Add Third Party"
  2. Select: "Pedestrian"
  3. Enter: "Tom Wilson"
  4. Select: "Yes" for injured
  5. Enter: "Broken Leg", "Leg fracture from impact"
  6. Enter: "2024-01-15" medical date
  7. Select: "No" for attorney
  8. Click: "[Save & Create Feature]"
  
Feature Modal Opens:
  1. Select: "UM" coverage
  2. Enter: $8,000 expense, $40,000 indemnity
  3. Select: "Edwin" as adjuster
  4. Click: "[Create Feature]"
  
Results:
  ? Third Party "Tom Wilson" in Third Party List
  ? Feature 04 in Feature Grid
    ?? Coverage: UM - 25,000
    ?? Claimant: Tom Wilson
    ?? Reserves: $8,000.00 / $40,000.00
    ?? Adjuster: Edwin
    ?? Edit/Delete buttons available
```

---

## ?? Troubleshooting

| Issue | Solution |
|-------|----------|
| Feature modal won't open | Verify "Yes" selected for injured |
| Button is disabled | Fill all required fields |
| Feature not in grid | Verify feature was saved (not cancelled) |
| Features have wrong number | Delete and recreate (will renumber) |
| Passenger deleted but feature remains | Check Step 5 review to see if persisted |

---

## ?? Feature Comparison

```
DRIVER (Step 3)              PASSENGER (Step 3)         THIRD PARTY (Step 4)
?????????????????????        ?????????????????????      ?????????????????????
? Injury Capture            ? Injury Capture           ? Injury Capture
? Attorney Option           ? Attorney Option          ? Attorney Option
? Feature Modal             ? Feature Modal NEW        ? Feature Modal NEW
? Feature Grid              ? Feature Grid NEW         ? Feature Grid NEW
? Edit Feature              ? Edit Feature NEW         ? Edit Feature NEW
? Delete Feature            ? Delete Feature NEW       ? Delete Feature NEW
? Auto-numbering            ? Auto-numbering NEW       ? Auto-numbering NEW
ClaimType="Driver"          ClaimType="Passenger"      ClaimType="ThirdParty"
```

---

## ?? Deployment Checklist

- [ ] Code reviewed and approved
- [ ] Build successful
- [ ] All tests passing
- [ ] Documentation reviewed
- [ ] UAT testing complete
- [ ] Stakeholders approved
- [ ] Deployment scheduled
- [ ] Backup created
- [ ] Deployment executed
- [ ] Post-deployment testing done
- [ ] Users notified
- [ ] Monitoring activated

---

## ?? Support

For questions or issues:
1. Check PASSENGER_THIRDPARTY_FEATURES.md for implementation details
2. Check WORKFLOW_VISUAL_ALL_PARTIES.md for visual workflows
3. Check IMPLEMENTATION_CHECKLIST.md for verification steps
4. Review this quick reference for common scenarios

---

## ?? Key Takeaways

? **Passengers** now have automatic feature creation for injuries  
? **Third Parties** now have automatic feature creation for injuries  
? **Bicyclist** added as third party type  
? **Feature grids** show all created features with edit/delete  
? **Auto-numbering** maintains feature numbers properly  
? **Data collection** properly aggregates all features by step  
? **Consistent UX** across all injury party types  

---

**Version**: 2.1.0  
**Status**: ? Production Ready  
**Last Updated**: 2024  

