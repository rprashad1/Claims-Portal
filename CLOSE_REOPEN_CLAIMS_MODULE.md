# Close and Reopen Claims Module

## Overview
This module enables users to close and reopen sub-claims (features) with full audit trail support.

## Business Rules

### Status Flow
1. **Open** ? **Closed** ? **Reopened** ? **Closed**
2. Open claims can only be closed
3. Closed claims can only be reopened
4. Reopened status is functionally the same as Open (users can perform actions)

### Claim Status Logic
- When **ALL** sub-claims are Closed ? Claim status = **Closed**
- When **ANY** sub-claim is Open or Reopened ? Claim status = **Open/Reopened**

### Reserve Handling
- **On Close**: Both Expense Reserve and Indemnity Reserve are set to **$0.00**
- **On Reopen**: User can choose:
  - Keep reserves at $0.00
  - Restore to previous values before closing
  - Set to default values: Expense = $500, Indemnity = $1,500

## Features

### Close Feature Modal
- Displays selected feature(s) summary
- Close Reason dropdown (required):
  - Below Deductible
  - Open in Error
  - Close with Negotiation
  - Close with Payment
  - Coverage Denial
  - Insured Request
  - Lack of Interest
- Shows current reserves (will be zeroed out)
- Multi-line remarks text box
- Warning about reserve reset

### Reopen Feature Modal
- Displays selected feature(s) summary
- Reopen Reason dropdown (required):
  - Additional Payment Required
  - Close in Error
  - New Claimant to be added
  - Subrogation
  - Litigation
- Reserve restoration options (radio buttons)
- Multi-line remarks text box

### Dynamic UI
- Action icon changes based on selected sub-claim status:
  - If selected features are Open ? Show "Close Feature" (red X icon)
  - If selected features are Closed ? Show "Reopen Feature" (green arrow icon)
  - If mixed status ? Show "Mixed Status" (disabled)

## Files Modified/Created

### New Files
1. `Components/Modals/CloseReopenFeatureModal.razor` - Main modal component
2. `Database/011_CloseReopen_SubClaims_AuditTrail.sql` - Database migration

### Modified Files
1. `Services/DatabaseClaimService.cs` - Added enhanced close/reopen methods
2. `Data/ClaimsPortalDbContext.cs` - Added SubClaimAudit entity and DbSet
3. `Components/Shared/SubClaimFullView.razor` - Integrated modal and dynamic buttons

## Database Changes

### SubClaims Table (New Columns)
- `CloseReason` (NVARCHAR(100)) - Reason for closing
- `ReopenReason` (NVARCHAR(100)) - Reason for reopening

### SubClaimAudits Table (New)
| Column | Type | Description |
|--------|------|-------------|
| SubClaimAuditId | BIGINT | Primary key |
| SubClaimId | BIGINT | FK to SubClaims |
| Action | NVARCHAR(50) | "Close" or "Reopen" |
| Reason | NVARCHAR(200) | Close/Reopen reason |
| Remarks | NVARCHAR(MAX) | User remarks |
| PreviousExpenseReserve | DECIMAL(18,2) | Reserve before action |
| PreviousIndemnityReserve | DECIMAL(18,2) | Reserve before action |
| NewExpenseReserve | DECIMAL(18,2) | Reserve after action |
| NewIndemnityReserve | DECIMAL(18,2) | Reserve after action |
| PerformedBy | NVARCHAR(100) | User who performed action |
| AuditDate | DATETIME | Timestamp |

### LookupCodes (New Entries)
- CloseReason: 7 predefined reasons
- ReopenReason: 5 predefined reasons

## API Methods (IDatabaseClaimService)

### Enhanced Close/Reopen with Audit Trail
```csharp
// Close with full details
Task<SubClaim> CloseSubClaimWithDetailsAsync(
    long subClaimId, 
    string reason, 
    string remarks, 
    string closedBy);

// Reopen with reserve options
Task<SubClaim> ReopenSubClaimWithDetailsAsync(
    long subClaimId, 
    string reason, 
    string remarks, 
    decimal expenseReserve, 
    decimal indemnityReserve, 
    string reopenedBy);

// Get previous reserves for restoration
Task<(decimal ExpenseReserve, decimal IndemnityReserve)> GetPreviousReservesAsync(
    long subClaimId);

// Add audit record
Task AddSubClaimAuditAsync(
    long subClaimId, 
    string action, 
    string reason, 
    string remarks, 
    decimal? previousExpenseReserve, 
    decimal? previousIndemnityReserve, 
    decimal? newExpenseReserve, 
    decimal? newIndemnityReserve, 
    string performedBy);
```

## Usage

### From SubClaimFullView
1. Select one or more sub-claims using checkboxes
2. The action toolbar appears showing the appropriate action:
   - "Close Feature" (for open/reopened features)
   - "Reopen Feature" (for closed features)
3. Click the action button to open the modal
4. Select a reason from the dropdown
5. Optionally enter remarks
6. For reopen: select reserve restoration option
7. Click Submit
8. Success message displays and grid refreshes

## Testing Checklist
- [ ] Select open feature ? Close button appears
- [ ] Select closed feature ? Reopen button appears
- [ ] Select mixed status features ? "Mixed Status" shows (disabled)
- [ ] Close feature ? Reserves reset to $0.00
- [ ] Close feature ? Audit record created
- [ ] Reopen with "Keep at $0" ? Reserves stay $0
- [ ] Reopen with "Restore previous" ? Reserves restored
- [ ] Reopen with "Default" ? Reserves set to $500/$1,500
- [ ] Close all features ? Claim status changes to Closed
- [ ] Reopen any feature ? Claim status changes to Open

## JavaScript Requirements
The modal uses Bootstrap's modal functionality. Ensure these JS functions exist:
```javascript
function ShowModal(modalId) {
    var modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}

function HideModal(modalId) {
    var modalElement = document.getElementById(modalId);
    var modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) modal.hide();
}
```
