# Claims Portal - Business Rules Implementation

## Document Information
| Item | Details |
|------|---------|
| **Feature** | Claim & Sub-Claim Status Management |
| **Version** | 1.0 |
| **Date** | January 2025 |
| **Status** | Implemented |

---

## Business Rules Overview

### Claim Lifecycle

```
???????????????????????????????????????????????????????????????????????????
?                        CLAIM STATUS LIFECYCLE                            ?
???????????????????????????????????????????????????????????????????????????

    ???????????????
    ?    FNOL     ?    (First Notice of Loss)
    ?   Process   ?    - No claim number yet
    ?  (Step 1-5) ?    - FNOL Status = 'D' (Draft) during entry
    ???????????????
           ?
           ? Submit Claim
    ???????????????
    ?   CLAIM     ?    - Claim Number assigned (CLM{date}{seq})
    ?   CREATED   ?    - Claim Status = 'O' (Open)
    ?  Status: O  ?    - All Sub-Claims Status = 'O' (Open)
    ???????????????
           ?
           ? Close All Sub-Claims
    ???????????????
    ?   CLAIM     ?    - Claim Status = 'C' (Closed)
    ?   CLOSED    ?    - All Sub-Claims Status = 'C' (Closed)
    ?  Status: C  ?
    ???????????????
           ?
           ? Reopen Any Sub-Claim
    ???????????????
    ?   CLAIM     ?    - Claim Status = 'O' (Open)
    ?  REOPENED   ?    - Reopened Sub-Claim Status = 'R' (Reopened)
    ?  Status: O  ?
    ???????????????
```

---

## Rule 1: FNOL Creates Open Claim

### Description
At the end of the FNOL process, when the user submits the claim:
- A **Claim Number** is assigned (format: `CLM{YYYYMMDD}{SEQUENCE}`)
- The **Claim Status** is set to **Open** ('O')
- The FNOL Status changes from Draft ('D') to represent an active claim

### Implementation
```csharp
// In DatabaseClaimService.SaveCompleteFnolAsync()
fnol.ClaimNumber = claimNumber;  // e.g., CLM2025010800001
fnol.FnolStatus = 'O';           // O = Open
fnol.ClaimCreatedDate = DateTime.Now;
```

### Trigger
- User clicks "Submit Claim" on Step 5 (Review & Save)

---

## Rule 2: Sub-Claims Created as Open

### Description
When sub-claims (features) are created during the FNOL process:
- Each sub-claim is assigned a **Feature Number** (sequential: 01, 02, 03...)
- Each sub-claim receives a **Sub-Claim Number** (format: `{ClaimNumber}-{FeatureNumber}`)
- Each sub-claim **Status** is set to **Open** ('O')
- The **Opened Date** is set to the current date/time

### Implementation
```csharp
// In DatabaseClaimService.CreateSubClaimAsync()
subClaim.SubClaimStatus = 'O';  // O = Open
subClaim.OpenedDate = DateTime.Now;
subClaim.SubClaimNumber = $"{subClaim.ClaimNumber}-{subClaim.FeatureNumber:D2}";
```

### Example
| Claim Number | Feature # | Sub-Claim Number | Status |
|--------------|-----------|------------------|--------|
| CLM2025010800001 | 01 | CLM2025010800001-01 | Open |
| CLM2025010800001 | 02 | CLM2025010800001-02 | Open |
| CLM2025010800001 | 03 | CLM2025010800001-03 | Open |

---

## Rule 3: Claim Closes When All Sub-Claims Close

### Description
The claim status automatically changes to **Closed** ('C') when:
- **ALL** sub-claims associated with the claim have status = 'C' (Closed)

### Implementation
```csharp
// In DatabaseClaimService.UpdateClaimStatusBasedOnSubClaimsAsync()
bool allClosed = subClaims.All(sc => sc.SubClaimStatus == 'C');

if (allClosed)
{
    fnol.FnolStatus = 'C'; // C = Closed
}
```

### Trigger
- Automatically called after any sub-claim is closed

---

## Rule 4: Sub-Claims Can Be Closed and Reopened

### Description
Each sub-claim can be:
- **Closed** at any time (Status changes to 'C')
- **Reopened** after being closed (Status changes to 'R')

### Close Sub-Claim Implementation
```csharp
public async Task<Data.SubClaim> CloseSubClaimAsync(long subClaimId, string closedBy)
{
    subClaim.SubClaimStatus = 'C';  // C = Closed
    subClaim.ClosedDate = DateTime.Now;
    subClaim.ModifiedBy = closedBy;
    
    // Check if all sub-claims are now closed
    await UpdateClaimStatusBasedOnSubClaimsAsync(subClaim.FnolId, closedBy);
}
```

### Reopen Sub-Claim Implementation
```csharp
public async Task<Data.SubClaim> ReopenSubClaimAsync(long subClaimId, string reopenedBy)
{
    subClaim.SubClaimStatus = 'R';  // R = Reopened
    subClaim.ClosedDate = null;     // Clear closed date
    subClaim.ModifiedBy = reopenedBy;
    
    // Claim status must be set to Open
    await UpdateClaimStatusBasedOnSubClaimsAsync(subClaim.FnolId, reopenedBy);
}
```

---

## Rule 5: Reopening Any Sub-Claim Reopens the Claim

### Description
When any sub-claim is reopened:
- The **Claim Status** automatically changes to **Open** ('O')
- This ensures the claim reflects that there is active work to be done

### Implementation
```csharp
// In DatabaseClaimService.UpdateClaimStatusBasedOnSubClaimsAsync()
bool anyOpenOrReopened = subClaims.Any(sc => 
    sc.SubClaimStatus == 'O' || sc.SubClaimStatus == 'R');

if (anyOpenOrReopened)
{
    fnol.FnolStatus = 'O'; // O = Open
}
```

---

## Status Code Reference

### Claim Status (FNOL.FnolStatus)
| Code | Name | Description |
|------|------|-------------|
| D | Draft | FNOL in progress, not yet submitted |
| O | Open | Active claim with at least one open/reopened sub-claim |
| C | Closed | All sub-claims are closed |

### Sub-Claim Status (SubClaim.SubClaimStatus)
| Code | Name | Description |
|------|------|-------------|
| O | Open | Active sub-claim, created and not yet closed |
| C | Closed | Sub-claim has been closed |
| R | Reopened | Previously closed sub-claim that has been reopened |

---

## UI Implementation

### Sub-Claims Grid Actions

When sub-claims are selected in the grid:
- **Close Feature** button appears for Open/Reopened sub-claims
- **Reopen Feature** button appears for Closed sub-claims

### Visual Feedback
- Processing spinner shows during status change
- Success/error message displayed after action
- Status badge updates immediately after change
- Claim header status refreshes automatically

### Badge Colors
| Status | Badge Class | Color |
|--------|-------------|-------|
| Open | bg-success | Green |
| Closed | bg-secondary | Gray |
| Reopened | bg-warning | Yellow |

---

## Database Changes

### SubClaims Table New Fields
```sql
-- Added to track who closed/reopened sub-claims
ClosedBy NVARCHAR(100) NULL,
ReopenedBy NVARCHAR(100) NULL,
ReopenedDate DATETIME NULL,
ModifiedTime TIME NULL
```

---

## Service Methods

### IDatabaseClaimService Interface
```csharp
// Sub-Claim Status Management
Task<SubClaim> CloseSubClaimAsync(long subClaimId, string closedBy);
Task<SubClaim> ReopenSubClaimAsync(long subClaimId, string reopenedBy);
Task<List<SubClaim>> CloseMultipleSubClaimsAsync(List<long> subClaimIds, string closedBy);
Task<SubClaim?> GetSubClaimAsync(long subClaimId);
Task<SubClaim> UpdateSubClaimAsync(SubClaim subClaim);

// Claim Status Management
Task<bool> UpdateClaimStatusBasedOnSubClaimsAsync(long fnolId, string modifiedBy);
Task<string> GetClaimStatusAsync(long fnolId);
```

---

## Testing Scenarios

### Scenario 1: New Claim Creation
1. Complete FNOL Steps 1-5
2. Create 3 sub-claims
3. Submit claim
4. **Expected**: Claim Status = Open, All Sub-Claims = Open

### Scenario 2: Close All Sub-Claims
1. Open existing claim with 3 sub-claims
2. Select all sub-claims
3. Click "Close Feature"
4. **Expected**: All Sub-Claims = Closed, Claim Status = Closed

### Scenario 3: Reopen One Sub-Claim
1. Open a closed claim
2. Select one closed sub-claim
3. Click "Reopen Feature"
4. **Expected**: Selected Sub-Claim = Reopened, Claim Status = Open

### Scenario 4: Partial Close
1. Open claim with 3 open sub-claims
2. Close 2 of 3 sub-claims
3. **Expected**: 2 Sub-Claims = Closed, 1 = Open, Claim Status = Open

---

## Summary

| Event | Claim Status | Sub-Claim Status |
|-------|--------------|------------------|
| Submit FNOL | Open | All Open |
| Close some sub-claims | Open | Mix of Open/Closed |
| Close ALL sub-claims | Closed | All Closed |
| Reopen ANY sub-claim | Open | Mix (Reopened + Closed) |

---

**Document End**
