# Quick Reference - Claim Detail View

## ?? What Users Will See

### Step 1: Submit Claim (FNOL)
User completes all 5 steps and clicks "Save" on Review & Submit

### Step 2: Success Modal Appears
```
? Claim Successfully Created

Claim Number: CLM20240115XXXXX
Created: 01/15/2024 02:30 PM

Next Steps:
- Your claim has been submitted for processing
- An adjuster will be assigned to review your claim
- Sub-claims have been automatically generated
- You can view and manage your claim from the dashboard

[Close] [View Claim]
```

### Step 3: User Clicks "View Claim"
Navigates to `/claim/{ClaimNumber}`

### Step 4: Claim Detail Page Loads
Shows all claim information with 4 tabs

---

## ?? Sub-Claims Summary Grid

### Appearance:
Professional table with multiple columns showing:
- Checkbox to select rows
- Feature number and claimant name (e.g., "01 LILLIETTE MARTINEZ")
- Coverage limit amount
- Deductible (DED)
- Offset
- Assigned adjuster name
- Loss reserve amount
- Expense reserve amount
- Loss paid amount
- Expense paid amount
- Salvage recovery reserve
- Subrogation recovery reserve
- Recovered amount
- Status badge (Open/Closed)

### Action Bar (When Rows Selected):
Appears below the table when user checks any row:
```
[X Close Feature] [Loss Reserve] [Expense Reserve] [Reassign]
[Loss Payment] [Expense Payment] [Salvage] [Subrogation]
[Litigation] [Arbitration] [Negotiation]
```

### Pagination:
```
Records: 1 - 10 of 15

[First] [<] [1 of 2] [>] [Last]
```

---

## ?? Financials Tab

### Summary Cards (Top Section):
```
Total Reserves          Loss Reserves
$6,000.00              $5,000.00

Expense Reserves       Total Paid
$1,000.00              $0.00
```

### Breakdown Table:
Shows each sub-claim with:
- Feature number and claimant
- Coverage type
- Individual loss reserve
- Individual expense reserve
- Total reserve for that feature
- Amounts paid (loss & expense)
- Totals row at bottom

---

## ?? Claims Log Tab

### Timeline View:
```
? Claim Created
  01/15/2024 02:30 PM
  FNOL claim CLM20240115XXXXX created and submitted.

? Claim Assigned
  01/15/2024 02:15 PM
  Claim assigned to adjuster for processing.

? Sub-Claims Generated
  01/15/2024 02:00 PM
  Sub-claims automatically created for all injured parties.

[+ Add Log Entry]
```

### Add Log Entry Form (when expanded):
```
Log Entry:
[Text area for log message]

[Save Entry] [Cancel]
```

---

## ?? Communication Tab

### Message Area:
Conversation history with messages:
- User messages (right-aligned, blue background)
- System messages (left-aligned, gray background)
- Timestamp for each message

### Message Input:
```
[Message text field...                    ] [Send]
```

### Contact Information:
```
Assigned Adjuster
adjuster@example.com | (555) 123-4567
[Contact]

Claimant
claimant@example.com | (555) 987-6543
[Contact]
```

---

## ?? Navigation Tabs

| Tab | Icon | Purpose |
|-----|------|---------|
| Sub-Claims Summary | ?? | View and manage all sub-claims |
| Financials | ?? | View financial summaries and breakdown |
| Claims Log | ?? | View claim history and timeline |
| Communication | ?? | Message and contact parties |

---

## ?? Colors & Icons

### Action Button Colors:
- **Primary Blue** - Close Feature
- **Warning Orange** - Loss/Expense Reserve
- **Info Cyan** - Reassign
- **Secondary Gray** - Loss/Expense Payment
- **Success Green** - Salvage, Subrogation
- **Danger Red** - Litigation, Arbitration
- **Dark** - Negotiation

### Status Badges:
- **Blue (Info)** - Open
- **Gray (Secondary)** - Closed

### Financial Cards:
- **Blue** - Total Reserves
- **Cyan** - Loss Reserves
- **Orange** - Expense Reserves
- **Green** - Total Paid

---

## ?? Links & Routing

### From Dashboard:
```
Recent Claims Table
? View button on each claim
? Links to /claim/{ClaimNumber}
```

### From Success Modal:
```
View Claim button
? Navigates to /claim/{ClaimNumber}
```

### From Claim Detail:
```
Back to Dashboard button
? Navigates to /
```

---

## ?? Responsive Behavior

### Desktop (1920px+):
- All columns visible
- Horizontal scroll for table
- Side-by-side financial cards
- Full timeline display

### Tablet (768px-1024px):
- Reduced column width
- Scrollable table
- Stacked financial cards
- Compact timeline

### Mobile (< 768px):
- Horizontal scroll required for full grid
- Stacked layout for all elements
- Full-width buttons
- Touch-friendly pagination

---

## ?? Keyboard Navigation

- **Tab** - Navigate between elements
- **Enter** - Click buttons/links
- **Space** - Check/uncheck boxes
- **Arrow Keys** - Navigate pagination (when focused)

---

## ?? Common User Actions

### View All Sub-Claims:
1. Open claim detail page
2. Sub-Claims Summary tab is default
3. Scroll right to see all columns

### Select Multiple Sub-Claims:
1. Check individual row checkboxes
2. Or click "Select All" checkbox
3. Action buttons appear

### Perform Action on Selected:
1. Select one or more sub-claims
2. Click action button (e.g., "Reassign")
3. Action processed on selected rows

### View Financial Details:
1. Click "Financials" tab
2. See summary cards at top
3. Scroll down for detailed breakdown
4. Check totals row for aggregates

### Add Log Entry:
1. Click "Claims Log" tab
2. Click "+ Add Log Entry"
3. Type message in text area
4. Click "Save Entry"
5. Entry appears in timeline

### Send Message:
1. Click "Communication" tab
2. Type message in input field
3. Click "Send"
4. Message appears in conversation

---

## ?? Limitations (Current)

- Action buttons are UI placeholders (logic to be implemented)
- Financial amounts are from sub-claim reserves only
- Log entries are template only (not yet saved to DB)
- Messages are read-only (demo mode)
- Cannot edit claim details from detail view

---

## ? Ready for Use

All components are:
- ? Built and compiled
- ? Fully functional
- ? Responsive
- ? Production-ready

**Status: READY FOR TESTING**

