# Deployment Checklist - Driver & Injury Workflow

## Pre-Deployment Verification

### Code Review
- [x] FnolStep3_DriverAndInjury.razor reviewed
- [x] SubClaimModal.razor reviewed
- [x] No breaking changes to other components
- [x] No deprecated methods used
- [x] Follows existing code patterns
- [x] Properly indented and formatted

### Build Status
- [x] Builds successfully without errors
- [x] No compiler warnings in modified files
- [x] All references resolved
- [x] No missing dependencies

### Backward Compatibility
- [x] Existing passenger functionality unchanged
- [x] Existing navigation unaffected
- [x] Previous data structures compatible
- [x] No breaking API changes

---

## Functional Testing

### Feature: Save Button

**Test Case 1: Button Visibility**
- [ ] Button visible when driver is injured
- [ ] Button hidden when driver is not injured
- [ ] Button always shows with consistent styling

**Test Case 2: Button Validation**
- [ ] Button disabled when driver name empty
- [ ] Button disabled when injury nature empty (if injured)
- [ ] Button disabled when medical date empty (if injured)
- [ ] Button disabled when injury description empty (if injured)
- [ ] Button disabled when attorney name empty (if attorney selected)
- [ ] Button disabled when attorney firm empty (if attorney selected)
- [ ] Button enabled when all required fields filled

**Test Case 3: Button Behavior**
- [ ] Click without injury ? DriverSaved = true, no modal
- [ ] Click with injury ? Feature modal opens automatically
- [ ] Modal opens with correct driver name in header
- [ ] Modal shows claimant name in alert

---

### Feature: Feature Modal

**Test Case 4: Modal Display**
- [ ] Modal shows correct claimant name
- [ ] Coverage dropdown populated correctly
- [ ] All 5 coverage options visible (BI, PD, PIP, APIP, UM)
- [ ] Reserve fields accept decimal input
- [ ] Adjuster dropdown has 4 options (Raj, Edwin, Constantine, Joan)

**Test Case 5: Modal Validation**
- [ ] Create button disabled when coverage empty
- [ ] Create button disabled when expense reserve = 0
- [ ] Create button disabled when indemnity reserve = 0
- [ ] Create button disabled when adjuster empty
- [ ] Create button enabled when all fields valid

**Test Case 6: Modal Summary**
- [ ] Summary box appears after all fields filled
- [ ] Summary shows correct coverage limits
- [ ] Summary shows correct reserve amounts
- [ ] Summary shows correct adjuster name
- [ ] Summary updates when values change

---

### Feature: Feature Grid

**Test Case 7: Grid Display**
- [ ] Grid appears after first feature created
- [ ] Grid title shows "Created Features/Sub-Claims"
- [ ] All columns present: Feature, Coverage/Limits, Claimant, Exp Res, Ind Res, Adjuster, Actions
- [ ] Feature numbers display correctly (01, 02, etc.)
- [ ] All values display correctly in grid

**Test Case 8: Feature Operations**
- [ ] Create Feature 01
  - [ ] Feature number = "01"
  - [ ] Coverage displays with limits (e.g., "BI - 100/300")
  - [ ] Reserves display as currency (e.g., "$5,000.00")
  - [ ] Adjuster displays correctly (e.g., "Raj")

- [ ] Create Feature 02
  - [ ] Feature number = "02"
  - [ ] Both features visible in grid

- [ ] Edit Feature 02
  - [ ] Edit button works
  - [ ] Modal opens with current values
  - [ ] Can change values
  - [ ] Changes save to grid
  - [ ] Feature number unchanged

- [ ] Delete Feature 02
  - [ ] Delete button works
  - [ ] Feature 02 removed
  - [ ] Feature 01 still numbered "01" (not renumbered)
  - [ ] Grid updates

---

### Feature: Navigation

**Test Case 9: Step Completion**
- [ ] Next button disabled before save
- [ ] Next button enabled after creating feature
- [ ] Next button enabled when no injury
- [ ] Can proceed to Step 4 after save
- [ ] Previous button works correctly

**Test Case 10: State Management**
- [ ] Changing driver type resets DriverSaved flag
- [ ] Selecting "No" for injured clears injury data
- [ ] Selecting "No" for attorney clears attorney data
- [ ] Features persist when switching between views
- [ ] Features clear when resetting step

---

### Feature: Data Integrity

**Test Case 11: Data Consistency**
- [ ] Driver info matches what user entered
- [ ] Injury info matches what user entered
- [ ] Attorney info matches what user entered
- [ ] Feature info matches modal entries
- [ ] All data preserved on modal close/reopen

**Test Case 12: Data Submission**
- [ ] GetDriver() returns correct driver info
- [ ] GetDriverInjury() returns correct injury info
- [ ] GetDriverAttorney() returns correct attorney info
- [ ] GetDriverSubClaims() returns all created features
- [ ] SubClaim objects have correct structure

---

## Integration Testing

### Test Case 13: Multi-Step Flow
```
Step 1 ? Step 2 ? Step 3 (THIS SECTION) ? Step 4 ? Step 5

? Complete Steps 1 and 2
? Enter all Step 3 data
? Create driver feature
? Proceed to Step 4
? Verify: Driver data passed correctly
? Verify: Feature data passed correctly
? Verify: All data in CurrentClaim object
```

### Test Case 14: Multiple Scenarios

**Scenario A: No Injury**
```
? Driver name: "Insured"
? Injured: No
? Click Save
? Verify: Modal doesn't open
? Verify: Grid doesn't show
? Verify: DriverSaved = true
? Verify: Next button enabled
? Proceed to Step 4
? Verify: Driver data intact
? Verify: No injury data
? Verify: No attorney data
? Verify: No features
```

**Scenario B: Injured, No Attorney**
```
? Driver name: "John Doe"
? Injured: Yes
? Injury details: Whiplash, 2024-01-15, Neck pain
? Attorney: No
? Click Save
? Verify: Modal opens
? Coverage: BI
? Reserves: 5000, 25000
? Adjuster: Raj
? Create Feature
? Verify: Feature 01 in grid
? Verify: Grid shows all values correctly
? Verify: DriverSaved = true
? Verify: Next button enabled
? Proceed to Step 4
? Verify: All data intact
```

**Scenario C: Injured with Attorney**
```
? Driver name: "Jane Smith"
? Injured: Yes
? Injury details: Fracture, 2024-01-16, Arm fracture
? Attorney: Yes
? Attorney: John Law, Smith Law, 555-1234, john@law.com, 123 Law St
? Click Save
? Verify: Modal opens
? Coverage: PD
? Reserves: 10000, 50000
? Adjuster: Edwin
? Create Feature
? Verify: Feature 01 in grid
? Verify: All values correct
? Verify: DriverSaved = true
? Verify: Next button enabled
? Proceed to Step 4
? Verify: All data including attorney intact
```

---

## Regression Testing

### Test Case 15: Existing Functionality
- [ ] Passengers section still works
- [ ] Can add passengers
- [ ] Can delete passengers
- [ ] Passenger injuries unaffected
- [ ] Steps 1, 2, 4, 5 unchanged

### Test Case 16: Browser Compatibility
- [ ] Works in Chrome
- [ ] Works in Firefox
- [ ] Works in Edge
- [ ] Works in Safari
- [ ] Modals display correctly
- [ ] Form styling intact
- [ ] No JavaScript errors

### Test Case 17: Responsive Design
- [ ] Desktop display correct
- [ ] Tablet display correct
- [ ] Mobile display correct
- [ ] Buttons accessible on all sizes
- [ ] Grid responsive on mobile

---

## Performance Testing

### Test Case 18: Performance
- [ ] Page loads quickly
- [ ] Modal opens without lag
- [ ] Grid renders smoothly with 5+ features
- [ ] Form input responsive
- [ ] No memory leaks on repeated open/close

---

## User Acceptance Testing

### Test Case 19: User Experience
- [ ] Flow is intuitive
- [ ] Save button clearly visible
- [ ] Feature creation is obvious
- [ ] Grid feedback is clear
- [ ] No confusing states
- [ ] Error messages helpful

### Test Case 20: Accessibility
- [ ] Form labels readable
- [ ] Button labels clear
- [ ] Color contrast sufficient
- [ ] Keyboard navigation works
- [ ] Screen reader compatible

---

## Sign-Off Checklist

### Development Team
- [ ] Code complete and reviewed
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Documentation updated
- [ ] No known issues

### QA Team
- [ ] All test cases passed
- [ ] No critical defects found
- [ ] No major defects found
- [ ] Performance acceptable
- [ ] User experience acceptable

### Product Owner
- [ ] Requirements met
- [ ] Workflow smooth
- [ ] Feature complete
- [ ] Ready for release

---

## Deployment Steps

1. **Pre-Deployment**
   - [ ] Pull latest code
   - [ ] Run full build
   - [ ] Verify no errors
   - [ ] Run all tests

2. **Deployment**
   - [ ] Deploy to staging
   - [ ] Verify build successful
   - [ ] Test on staging
   - [ ] Deploy to production
   - [ ] Verify production

3. **Post-Deployment**
   - [ ] Monitor logs
   - [ ] Monitor errors
   - [ ] Test key scenarios
   - [ ] Get user feedback
   - [ ] Document any issues

---

## Rollback Plan

If issues found post-deployment:

1. **Identify Issue**
   - [ ] Check logs
   - [ ] Reproduce error
   - [ ] Determine impact

2. **Decide Action**
   - [ ] If minor: Create hot fix
   - [ ] If major: Rollback to previous version

3. **Execute Rollback** (if needed)
   - [ ] Revert code
   - [ ] Revert database
   - [ ] Verify rollback
   - [ ] Communicate with users

---

## Success Criteria

? **Go/No-Go Decision**: READY FOR DEPLOYMENT

When all checkboxes are marked:
- Code quality: ? High
- Test coverage: ? Complete
- Performance: ? Acceptable
- User experience: ? Excellent
- Documentation: ? Complete
- Risk: ? Low

**Status**: ?? **DEPLOYMENT APPROVED**

---

## Post-Deployment Monitoring

### First Week
- [ ] Monitor error logs daily
- [ ] Check feature usage patterns
- [ ] Gather user feedback
- [ ] Track performance metrics
- [ ] Document any issues

### Ongoing
- [ ] Monitor feature adoption
- [ ] Track performance trends
- [ ] Respond to user feedback
- [ ] Plan next iteration

