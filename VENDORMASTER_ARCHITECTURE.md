# VendorMaster Architecture - Separate Table Design

## Overview

The VendorMaster table is **separate from EntityMaster** to optimize performance for vendor searches while keeping claim-related entities in their own table.

## ? Implementation Complete

All services now use the **VendorMaster** table:

| Component | Service | Table |
|-----------|---------|-------|
| **Vendor Master UI** | DatabaseVendorService | VendorMaster ? |
| **Hospital Search** | VendorSearchService | VendorMaster ? |
| **Attorney Search** | VendorSearchService | VendorMaster ? |
| **Authority Search** | VendorSearchService | VendorMaster ? |

---

## Why Separate Tables?

### Data Growth Analysis

| Table | Current Records | Annual Growth | 5-Year Projection |
|-------|-----------------|---------------|-------------------|
| **VendorMaster** | 50,000 | ~500/year | ~52,500 |
| **EntityMaster** | 500,000+ | ~50,000/year | ~750,000+ |

### Performance Benefits

| Scenario | EntityMaster (Mixed) | VendorMaster (Separate) |
|----------|---------------------|------------------------|
| Hospital Search | Scan 750K+ records | Scan 52K records |
| Attorney Lookup | Filter by PartyType | Direct index lookup |
| Caching Possible | No (too large) | Yes (small table) |

---

## Database Schema

### VendorMaster Table

```sql
VendorMaster
??? VendorId (PK, IDENTITY)
??? VendorType (Hospital, Defense Attorney, Plaintiff Attorney, etc.)
??? EntityType ('B' = Business, 'I' = Individual)
??? VendorName
??? DoingBusinessAs
??? FEINNumber
??? ContactName
??? BusinessPhone, MobilePhone, FaxNumber
??? Email, Website
??? EffectiveDate, TerminationDate
??? W9Received, W9ReceivedDate
??? SubjectTo1099, BackupWithholding
??? ReceivesBulkPayment, PaymentFrequency, PaymentDays
??? BarNumber, BarState (for Attorneys)
??? JurisdictionType (for Authorities)
??? VendorStatus ('Y' = Active, 'D' = Disabled)
??? CreatedDate, CreatedBy
??? ModifiedDate, ModifiedBy
??? LegacyEntityId (reference to old EntityMaster record)
```

### VendorAddress Table

```sql
VendorAddress
??? VendorAddressId (PK, IDENTITY)
??? VendorId (FK ? VendorMaster)
??? AddressType ('M' = Main, 'A' = Alternate, 'T' = Temporary)
??? StreetAddress, AddressLine2
??? City, State, ZipCode, Country
??? Phone, Fax, Email
??? AddressStatus
??? Audit fields
```

---

## Service Architecture

### Unified Architecture (All Using VendorMaster)

```
???????????????????????????????????????????????????????????????
?                    VendorMaster Table                        ?
?         (50,000 vendors - optimized for search)              ?
???????????????????????????????????????????????????????????????
                              ?
              ?????????????????????????????????
              ?                               ?
?????????????????????????????   ?????????????????????????????
?   DatabaseVendorService    ?   ?   VendorSearchService     ?
?   (Vendor Master UI)       ?   ?   (Search Modals)         ?
?   - Add/Edit/View vendors  ?   ?   - Hospital search       ?
?   - Address management     ?   ?   - Attorney search       ?
?   - Payment settings       ?   ?   - Authority search      ?
?????????????????????????????   ?????????????????????????????
              ?                               ?
              ?                               ?
?????????????????????????????   ?????????????????????????????
?   VendorMaster.razor      ?   ?   HospitalSearchModal     ?
?   (Admin page)            ?   ?   AttorneySearchModal     ?
?                           ?   ?   AuthorityModal          ?
?????????????????????????????   ?????????????????????????????
```

---

## Vendor Types

| VendorType | Description |
|------------|-------------|
| Hospital | Hospitals and medical centers |
| Medical Provider | Clinics, doctors offices |
| Defense Attorney | Insurance defense attorneys |
| Plaintiff Attorney | Claimant attorneys |
| Police Department | Police and sheriff departments |
| Fire Department | Fire stations |
| Towing Service | Tow truck companies |
| Repair Shop | Auto body shops |
| Rental Car Company | Car rental agencies |
| Insurance Carrier | Other insurance companies |
| Other | All other vendor types |

---

## Foreign Key References

### From Claimants Table

```sql
Claimants
??? HospitalVendorId ? VendorMaster.VendorId
??? AttorneyVendorId ? VendorMaster.VendorId
??? AttorneyEntityId ? EntityMaster.EntityId (legacy)
```

### From FnolAuthorities Table

```sql
FnolAuthorities
??? VendorId ? VendorMaster.VendorId
??? EntityId ? EntityMaster.EntityId (legacy)
```

---

## Migration from EntityMaster

The database script `019_Create_VendorMaster_Table.sql` includes:

1. **Create VendorMaster table** with optimized schema
2. **Create VendorAddress table** for vendor addresses
3. **Add FK columns** to Claimants and FnolAuthorities
4. **Migrate existing vendors** from EntityMaster where `EntityGroupCode = 'Vendor'`
5. **Add sample data** for testing

### Migration Query

```sql
INSERT INTO VendorMaster (...)
SELECT ... FROM EntityMaster
WHERE EntityGroupCode = 'Vendor'
```

---

## Backward Compatibility

### LegacyEntityId Column

Each vendor has a `LegacyEntityId` column that references the old EntityMaster record:

```csharp
public class VendorMasterEntity
{
    public long VendorId { get; set; }          // New primary key
    public long? LegacyEntityId { get; set; }   // Old EntityMaster.EntityId
}
```

---

## Files Modified

| File | Changes |
|------|---------|
| `Database\019_Create_VendorMaster_Table.sql` | Creates VendorMaster table and migrates data |
| `Data\ClaimsPortalDbContext.cs` | Added VendorMasterEntity, VendorAddressEntity |
| `Services\VendorSearchService.cs` | Uses VendorMaster for searches |
| `Services\DatabaseVendorService.cs` | **Updated** - Now uses VendorMaster for CRUD |
| `Models\Enums\VendorEnums.cs` | Added PoliceDepartment, FireDepartment |

---

## Execution Order

1. **Run database script**: `Database\019_Create_VendorMaster_Table.sql`
2. **Verify migration**: Check that vendors were migrated
3. **Test Vendor Master UI**: Add/Edit/View vendors
4. **Test search modals**: Hospital search, Attorney search, Authority search

---

## Performance Indexes

The VendorMaster table includes these indexes:

```sql
-- Fast lookup by vendor type
CREATE INDEX IX_VendorMaster_VendorType ON VendorMaster(VendorType) WHERE VendorStatus = 'Y'

-- Fast name search
CREATE INDEX IX_VendorMaster_VendorName ON VendorMaster(VendorName) 
    INCLUDE (VendorType, DoingBusinessAs, BusinessPhone, Email)

-- FEIN lookup
CREATE INDEX IX_VendorMaster_FEINNumber ON VendorMaster(FEINNumber) WHERE FEINNumber IS NOT NULL
```

---

## Summary

| Before | After |
|--------|-------|
| Vendor Master UI ? EntityMaster | Vendor Master UI ? VendorMaster |
| Search Modals ? VendorMaster | Search Modals ? VendorMaster |
| Two tables, potential sync issues | Single VendorMaster table |
| 500K+ records to scan | 50K vendor records to scan |
