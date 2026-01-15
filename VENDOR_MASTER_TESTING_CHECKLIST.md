# ? VENDOR MASTER - QUICK TESTING CHECKLIST

## ?? BEFORE TESTING
- [ ] Hard refresh browser (Ctrl+F5)
- [ ] Clear browser cache if needed
- [ ] Application running on F5
- [ ] Navigate to `/vendor-master` or use menu

---

## ?? TEST 1: ADD NEW VENDOR (5 minutes)

### Setup
- [ ] Click "Add New Vendor" button (top right)
- [ ] Modal opens
- [ ] Modal title shows "Add New Vendor"

### Fill Form
- [ ] Vendor Type: Select "Hospitals"
- [ ] Entity Type: Select "Business"
- [ ] Business Name: Type "Test Hospital"
- [ ] DBA: Type "TH"
- [ ] FEIN: Type "99-8877665"
- [ ] Effective Date: Pick today
- [ ] Contact Name: Type "John Manager"
- [ ] Business Phone: Type "(555) 123-4567"
- [ ] Email: Type "john@hospital.com"

### Add Address
- [ ] Scroll down to Addresses section
- [ ] Click "Add Address" button
- [ ] Address Type: Keep as "Main"
- [ ] Address 1: Type "456 Hospital Drive"
- [ ] City: Type "Chicago"
- [ ] State: Type "IL"
- [ ] ZIP: Type "60601"

### Configure Payment
- [ ] Scroll to Payment Information
- [ ] Toggle "Receives Bulk Payments" ON
- [ ] Verify payment section appears
- [ ] Select "Monthly" from Frequency
- [ ] Check dates 10 and 20
- [ ] Verify checkboxes stay checked

### Save
- [ ] Click "Save Vendor" button
- [ ] Modal closes
- [ ] New vendor appears in table

### Verify
- [ ] Vendor name shows "Test Hospital"
- [ ] Contact shows "John Manager"
- [ ] Status shows "Active" (green badge)
- [ ] Address shows "Chicago, IL 60601"

**Result**: ? PASS / ? FAIL

---

## ?? TEST 2: VIEW VENDOR (3 minutes)

### Setup
- [ ] Find "Boston Medical Center" in table
- [ ] Click "View" button (eye icon)
- [ ] Modal opens
- [ ] Modal title shows "View Vendor"

### Verify Read-Only
- [ ] Click Vendor Type dropdown - should NOT open
- [ ] Try typing in Name field - should NOT accept text
- [ ] Try toggling W9 Received - should NOT toggle
- [ ] Try changing Address Type - should NOT allow change
- [ ] Try clicking "Add Address" - button should not exist

### Verify Button
- [ ] Save button should NOT be visible
- [ ] Only "Close" button should be visible

### Close Modal
- [ ] Click "Close" button
- [ ] Modal closes
- [ ] Back to vendor list

### Verify No Changes
- [ ] Vendor data unchanged from before

**Result**: ? PASS / ? FAIL

---

## ?? TEST 3: EDIT VENDOR (5 minutes)

### Setup
- [ ] Find "Boston Medical Center" in table
- [ ] Click "Edit" button (pencil icon)
- [ ] Modal opens
- [ ] Modal title shows "Edit Vendor"

### Modify Data
- [ ] Note original Contact Name: "Dr. Smith"
- [ ] Change Contact Name to "Dr. Jane Smith"
- [ ] Change Mobile Phone to "(555) 888-7777"

### Save Changes
- [ ] Click "Save Vendor" button
- [ ] Modal closes
- [ ] Back to vendor list

### Verify Changes
- [ ] Find "Boston Medical Center" again
- [ ] Click "View"
- [ ] Verify Contact Name is now "Dr. Jane Smith"
- [ ] Verify Mobile Phone is "(555) 888-7777"
- [ ] Click "Close"

**Result**: ? PASS / ? FAIL

---

## ?? TEST 4: EDIT WITHOUT SAVING (3 minutes)

### Setup
- [ ] Find "Boston Medical Center" in table
- [ ] Click "Edit" button
- [ ] Modal opens

### Make Changes Without Saving
- [ ] Change Contact Name to "TEMP NAME"
- [ ] Change Email to "temp@temp.com"
- [ ] Click X button (close without Save)
- [ ] Modal closes

### Verify Changes NOT Saved
- [ ] Click "Edit" on same vendor again
- [ ] Verify Contact Name is still "Dr. Jane Smith" (or original)
- [ ] Verify Email is still "contact@bmc.com" (original)
- [ ] Click "Close"

**Result**: ? PASS / ? FAIL

---

## ?? TEST 5: BULK PAYMENTS TOGGLE (3 minutes)

### Setup
- [ ] Click "Add New Vendor"
- [ ] Fill minimal required fields (Name, Type, Entity)

### Test Toggle OFF (Initial State)
- [ ] Scroll to Payment Information
- [ ] Verify "Receives Bulk Payments" is OFF
- [ ] Verify payment section is HIDDEN
- [ ] No Frequency dropdown visible

### Test Toggle ON
- [ ] Toggle "Receives Bulk Payments" ON
- [ ] Verify payment section APPEARS immediately
- [ ] Verify Frequency dropdown is visible
- [ ] No dates/days selected yet

### Test Monthly
- [ ] Select "Monthly" from Frequency
- [ ] Verify date picker appears with days 1-31
- [ ] Verify "Last Day of Month" option exists
- [ ] Check dates 5, 10, 15
- [ ] Verify checkboxes stay checked

### Test Switch to Weekly
- [ ] Select "Weekly" from Frequency
- [ ] Verify date checkboxes are CLEARED
- [ ] Verify day picker appears (Mon-Fri)
- [ ] Select "Monday" and "Friday"

### Test Toggle OFF
- [ ] Toggle "Receives Bulk Payments" OFF
- [ ] Verify payment section DISAPPEARS
- [ ] Scroll down - no more payment section

**Result**: ? PASS / ? FAIL

---

## ?? TEST 6: DISABLE/ENABLE (3 minutes)

### Setup
- [ ] Find "Boston Medical Center" in table
- [ ] Verify Status badge shows "Active" (green)

### Test Disable
- [ ] Click "Disable" button (red)
- [ ] Verify Status badge changes to "Disabled" (red)
- [ ] Verify "Disable" button changes to "Enable" (green)

### Test Enable
- [ ] Click "Enable" button (green)
- [ ] Verify Status badge changes to "Active" (green)
- [ ] Verify "Enable" button changes to "Disable" (red)

### Test Toggle Multiple Times
- [ ] Click Disable
- [ ] Click Enable
- [ ] Click Disable
- [ ] Verify all changes take effect

**Result**: ? PASS / ? FAIL

---

## ?? TEST 7: SEARCH & FILTERS (3 minutes)

### Setup
- [ ] You should have vendors in the table

### Test Name Search
- [ ] Clear search box (if any text)
- [ ] Type "Boston"
- [ ] Click Search
- [ ] Verify only Boston Medical Center appears

### Test FEIN Search
- [ ] Clear search
- [ ] Type "12-3456789"
- [ ] Click Search
- [ ] Verify Boston Medical Center appears

### Test Type Filter
- [ ] Clear search
- [ ] Select "Hospitals" from Type dropdown
- [ ] Click Search
- [ ] Verify hospital vendors appear

### Test Status Filter
- [ ] Search for vendor you disabled
- [ ] Select "Disabled" from Status dropdown
- [ ] Click Search
- [ ] Verify disabled vendor appears
- [ ] Select "Active"
- [ ] Verify disabled vendor disappears

**Result**: ? PASS / ? FAIL

---

## ?? TEST 8: ADDRESS MANAGEMENT (3 minutes)

### Setup
- [ ] Click "Add New Vendor"
- [ ] Fill basic info
- [ ] Scroll to Addresses

### Test Add Multiple Addresses
- [ ] Click "Add Address" - Address appears
- [ ] Fill Address 1 details
- [ ] Click "Add Address" - Second address appears
- [ ] Fill Address 2 details (type = "Temporary")
- [ ] Click "Add Address" - Third address appears
- [ ] Fill Address 3 details (type = "Alternate")
- [ ] Verify "Add Address" button disappears (max 3)

### Test Remove Address
- [ ] Click "Remove" on Address 2
- [ ] Verify Address 2 is gone
- [ ] Verify "Add Address" button reappears
- [ ] Click "Add Address" again

### Test Address Types
- [ ] Verify Address 1 type = "Main"
- [ ] Verify Address 2 type = "Temporary"
- [ ] Verify Address 3 type = "Alternate"
- [ ] Change one to "Temporary"
- [ ] Verify change takes effect

**Result**: ? PASS / ? FAIL

---

## ?? SUMMARY

Total Tests: 8
- [ ] Test 1 - Add New Vendor: ?/?
- [ ] Test 2 - View Vendor: ?/?
- [ ] Test 3 - Edit Vendor: ?/?
- [ ] Test 4 - Edit Without Save: ?/?
- [ ] Test 5 - Bulk Payments: ?/?
- [ ] Test 6 - Disable/Enable: ?/?
- [ ] Test 7 - Search & Filters: ?/?
- [ ] Test 8 - Address Management: ?/?

**Overall Result**: ? PASS / ? FAIL

---

## ?? COMMON ISSUES & SOLUTIONS

### Issue: Modal doesn't appear
- **Solution**: Hard refresh (Ctrl+F5), clear cache

### Issue: Fields don't respond to clicks
- **Solution**: Check if in View mode (fields should be disabled)

### Issue: Bulk Payments section doesn't show
- **Solution**: Make sure toggle is turned ON

### Issue: Changes were saved when I didn't click Save
- **Solution**: You must have clicked Save - check the modal title

### Issue: Data disappeared after page refresh
- **Solution**: This is expected - data is in-memory only (not database yet)

---

## ?? IF TESTS FAIL

1. **Check browser console** for JavaScript errors (F12)
2. **Hard refresh** the page (Ctrl+F5)
3. **Check for typos** in field entries
4. **Verify application is running** (look for spinner or errors)
5. **Document the exact steps** that caused the failure

---

**Ready to Test?**

Press F5 and navigate to Vendor Master to begin! ?
