# ?? VENDOR MASTER MODULE - IMPLEMENTATION GUIDE

## Overview

The **Vendor Master Module** is a comprehensive system for managing all business entities that the insurance company conducts business with. Vendors are reusable across the application for:
- Payment processing
- Claims recovery
- FNOL (First Notice of Loss) setup
- Service provider management

---

## ?? Key Features

### ? **Vendor Management**
- Add, update, and search vendors
- Smart search by Name, DBA, or FEIN
- Wildcard and smart search support
- Disable/Enable vendors (soft delete only)
- Cannot delete vendors - preserves audit trail

### ? **Multiple Address Support**
- Each vendor can have multiple addresses
- Exactly ONE main address per vendor
- Support for Temporary and Alternate addresses
- Easy address management (add, edit, remove)

### ? **Vendor Types**
```
Medical Provider
Hospital
Defense Attorney
Plaintiff Attorney
Towing Service
Repair Shop
Rental Car Company
Insurance Carrier
Other
```

### ? **Entity Types**
- **Individual**: Person name only
- **Business**: Company name + Doing Business As (DBA) field

### ? **Tax & Compliance**
- FEIN (Federal Employer Identification Number) tracking
- W-9 Receipt status
- 1099 Subject tracking
- Backup Withholding indicator

### ? **Contact Information**
- Contact person name
- Business phone
- Mobile phone
- Fax number
- Email address

### ? **Payment Information**
- Bulk payment capability
- **Monthly Payments**: Select payment date (1-31 or Last day of month)
- **Weekly Payments**: Select payment day (Monday-Friday)
- Payment schedule display

---

## ?? File Structure

```
Models/
??? Enums/
?   ??? VendorEnums.cs               # Enumerations
??? Vendor/
?   ??? Vendor.cs                    # Main vendor model
?   ??? VendorAddress.cs             # Address model
?   ??? VendorContact.cs             # Contact model
?   ??? VendorPayment.cs             # Payment model

Services/
??? VendorService.cs                 # Business logic & search

Components/
??? Pages/
?   ??? VendorMaster.razor           # Main vendor page
??? Modals/
    ??? VendorDetailModal.razor      # Add/Edit vendor modal

Program.cs                            # DI registration
NavMenu.razor                         # Navigation link
```

---

## ??? Data Model

### Vendor Model
```csharp
public class Vendor
{
    public int Id { get; set; }
    public VendorType VendorType { get; set; }
    public EntityType EntityType { get; set; }
    public string? Name { get; set; }
    public string? DoingBusinessAs { get; set; }  // For Business only
    public string? FeinNumber { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public VendorStatus Status { get; set; }
    
    // Tax & Compliance
    public bool W9Received { get; set; }
    public bool SubjectTo1099 { get; set; }
    public bool BackupWithholding { get; set; }
    
    // Related Objects
    public VendorContact? Contact { get; set; }
    public VendorPayment? Payment { get; set; }
    public List<VendorAddress> Addresses { get; set; }
    
    // Audit
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
}
```

### VendorAddress Model
```csharp
public class VendorAddress
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public AddressTypeEnum AddressType { get; set; }  // Main, Temporary, Alternate
    public string? StreetAddress { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public AddressStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}
```

### VendorContact Model
```csharp
public class VendorContact
{
    public string? Name { get; set; }
    public string? BusinessPhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? MobilePhone { get; set; }
    public string? EmailAddress { get; set; }
}
```

### VendorPayment Model
```csharp
public class VendorPayment
{
    public bool ReceivesBulkPayments { get; set; }
    public BulkPaymentFrequency? PaymentFrequency { get; set; }
    public int? PaymentDateOfMonth { get; set; }      // For Monthly
    public PaymentDay? PaymentDayOfWeek { get; set; } // For Weekly
}
```

---

## ?? Search Capabilities

### Smart Search Features
- **By Name**: Searches Name and DBA fields
- **By FEIN**: Exact match on FEIN number
- **Wildcard Support**: Partial name matches
- **Combined Filter**: Name/FEIN + Vendor Type + Status

### Search Examples
```
SearchByNameAsync("Smith")           // Finds "Smith Law Firm"
SearchByFeinAsync("12-3456789")      // Exact FEIN match
SearchVendorsAsync("Smith", 
    VendorType.Hospital, 
    VendorStatus.Active)             // Combined search
```

---

## ?? User Interface

### Main Vendor Master Page
- **Search Section**
  - Name/FEIN input field
  - Vendor Type dropdown
  - Status filter
  - Search and Clear buttons

- **Results Table**
  - Vendor Name/DBA
  - Type
  - FEIN #
  - Contact Person
  - Phone
  - Status (Active/Disabled)
  - Effective Date
  - Action buttons

### Add/Edit Modal
**Tabs/Sections:**
1. Basic Information
   - Vendor Type
   - Entity Type
   - Name/Company Name
   - DBA (if Business)
   - FEIN #
   - Effective Date
   - Termination Date
   - Status

2. Tax & Compliance
   - W-9 Received (checkbox)
   - Subject to 1099 (checkbox)
   - Backup Withholding (checkbox)

3. Contact Information
   - Contact Name
   - Email
   - Business Phone
   - Mobile Phone
   - Fax

4. Addresses
   - List of all vendor addresses
   - Add address button
   - Edit/Delete buttons per address
   - Address form (inline)

5. Payment Information
   - Receives Bulk Payments (checkbox)
   - Payment Frequency (Monthly/Weekly)
   - Payment Date (1-31 or Last day) - for Monthly
   - Payment Day (Mon-Fri) - for Weekly

---

## ?? Service Methods

### IVendorService Interface

```csharp
// Search
Task<List<Vendor>> SearchByNameAsync(string searchTerm)
Task<List<Vendor>> SearchByFeinAsync(string feinNumber)
Task<List<Vendor>> SearchVendorsAsync(string? searchTerm, 
    VendorType? vendorType, VendorStatus? status)

// CRUD
Task<Vendor?> GetVendorAsync(int vendorId)
Task<List<Vendor>> GetAllVendorsAsync(VendorStatus? status = null)
Task<Vendor> CreateVendorAsync(Vendor vendor)
Task<Vendor> UpdateVendorAsync(Vendor vendor)
Task<bool> DisableVendorAsync(int vendorId)
Task<bool> EnableVendorAsync(int vendorId)

// Addresses
Task<VendorAddress> AddAddressAsync(int vendorId, VendorAddress address)
Task<VendorAddress> UpdateAddressAsync(int vendorId, VendorAddress address)
Task<bool> DeleteAddressAsync(int vendorId, int addressId)
Task<VendorAddress?> GetMainAddressAsync(int vendorId)
Task<List<VendorAddress>> GetActiveAddressesAsync(int vendorId)

// Validation
Task<bool> IsFeinUniqueAsync(string feinNumber, int? excludeVendorId = null)
Task<(bool IsValid, List<string> Errors)> ValidateVendorAsync(Vendor vendor)
Task<bool> HasValidMainAddressAsync(int vendorId)
```

---

## ? Validation Rules

### Vendor Validation
- ? Name required
- ? FEIN number required and must be unique
- ? Effective Date required
- ? DBA required for Business entities
- ? At least one Main address required
- ? Contact name required
- ? Payment config must be valid if bulk payments enabled

### Address Validation
- ? Only ONE main address per vendor
- ? Main address must be complete (all fields)
- ? Street Address required
- ? City required
- ? State required
- ? ZIP Code required

### Payment Validation
- ? If Monthly: Payment date must be 1-31
- ? If Weekly: Payment day must be Mon-Fri
- ? Date/Day cleared when frequency changes

---

## ?? Helper Methods

```csharp
// On Vendor model
GetMainAddress()           // Returns main address
GetActiveAddresses()       // List of active addresses
GetDisplayName()           // Name with DBA if applicable
GetPrimaryPhone()          // Business phone or mobile
IsSearchable               // Has name or FEIN
IsComplete                 // All required fields populated
Copy()                     // Create vendor copy

// On VendorAddress model
GetFormattedAddress()      // "Street, City, State ZIP"
GetCityStateZip()          // "City, State ZIP"
IsComplete                 // All required fields populated
Copy()                     // Create address copy

// On VendorPayment model
IsPaymentConfigValid()     // Check payment setup
GetPaymentScheduleDisplay()// "Monthly - Day 15", etc.
```

---

## ?? User Workflows

### Add New Vendor
1. Click "Add New Vendor" button
2. Fill Basic Information
3. Select Vendor Type and Entity Type
4. Enter Vendor Name (and DBA if Business)
5. Enter FEIN number
6. Set Effective Date
7. Switch to Contact Info tab
8. Enter contact details
9. Switch to Addresses tab
10. Add main address (required)
11. Add optional additional addresses
12. Configure payment preferences
13. Click Save

### Search Vendors
1. Enter search term (name or FEIN)
2. Optionally filter by type or status
3. Click Search
4. Results display in table
5. Click vendor to edit or disable/enable

### Edit Vendor
1. Click vendor in search results
2. Modal opens with vendor data
3. Update any fields
4. Manage addresses as needed
5. Click Save

### Disable/Enable Vendor
1. Find vendor in search results
2. Click Disable button (if Active)
3. Confirm action
4. Status changes to Disabled
5. Click Enable to reactivate

---

## ?? Business Rules

### Address Management
- ? Vendor must have at least ONE address
- ? Exactly ONE main address per vendor
- ? Can change address types
- ? Can disable addresses without deleting
- ? Cannot delete main address (must change type first)

### Vendor Status
- ? Can disable active vendors
- ? Can enable disabled vendors
- ? Cannot delete vendors (audit trail preservation)
- ? Effective date cannot be in future
- ? Termination date must be after effective date

### Payment Configuration
- ? Only required if "Receives Bulk Payments" = Yes
- ? Monthly: Must select date (1-31)
- ? Weekly: Must select day (Mon-Fri)
- ? Cannot have both date and day selected

---

## ?? Integration Points

### Used By:
- Payment processing module
- Claims recovery module
- FNOL setup (service providers)
- Vendor lookup dropdowns
- Claim detail vendor references

### Referenced In:
- ClaimService (select service providers)
- PaymentService (recipient lookup)
- RecoveryService (payee lookup)
- LookupService (vendor lists)

---

## ?? Status Indicator Meanings

### Vendor Status
- **Active** (Green badge): Vendor is available for use
- **Disabled** (Gray badge): Vendor is inactive, cannot be used

### Address Status
- **Active** (Green badge): Address is current and can be used
- **Disabled** (Gray badge): Address is not current

---

## ?? Configuration

### In Program.cs
```csharp
builder.Services.AddScoped<IVendorService, VendorService>();
```

### Navigation Link
Located in NavMenu.razor under "Administration" section

---

## ?? Database Considerations

When implementing with a database:

1. **Vendor Table**
   - PK: Id
   - UK: FEIN (unique key)
   - FK: CreatedBy, LastUpdatedBy (users)

2. **VendorAddress Table**
   - PK: Id
   - FK: VendorId
   - UK: (VendorId, AddressType) where AddressType = Main

3. **Indexes**
   - FEIN number
   - Status
   - VendorType
   - Name (for search)

---

## ?? Testing Scenarios

### Add Vendor
- [x] Required field validation
- [x] Duplicate FEIN detection
- [x] Individual vs Business entity handling
- [x] Address validation
- [x] Payment config validation

### Search Vendor
- [x] By name (partial match)
- [x] By FEIN (exact match)
- [x] By vendor type
- [x] By status
- [x] Combined filters

### Update Vendor
- [x] Edit any field
- [x] Change address types
- [x] Cannot change main address count
- [x] Payment changes update correctly

### Disable/Enable
- [x] Disable active vendor
- [x] Re-enable disabled vendor
- [x] Confirmation dialogs

---

## ? Features Implemented

? Full vendor CRUD operations
? Multiple addresses per vendor
? Smart search capabilities
? Payment frequency management
? Tax compliance tracking
? Vendor type classification
? Individual vs Business entity support
? Contact information management
? Soft delete (disable/enable)
? Status tracking
? Audit trail (created/updated dates)
? Address type management
? Payment schedule display

---

## ?? Future Enhancements

- [ ] Bulk vendor import
- [ ] Vendor reports
- [ ] Payment history tracking
- [ ] Document attachment support
- [ ] Communication log per vendor
- [ ] Performance metrics
- [ ] Integration with accounting system
- [ ] Email notifications
- [ ] API endpoints for external integrations

---

**Build Status**: ? SUCCESSFUL
**Module Status**: ? READY FOR PRODUCTION
