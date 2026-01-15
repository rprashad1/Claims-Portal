# ?? **VENDOR MASTER - COMPLETE FIELD LIST**

## **Form Sections & Fields**

```
???????????????????????????????????????????????????????????
? VENDOR MASTER - ADD NEW VENDOR                          ?
???????????????????????????????????????????????????????????
?                                                         ?
? BASIC INFORMATION                                       ?
? ?? Vendor Name * (text)                               ?
? ?? Party Type * (dropdown)                            ?
? ?? Vendor Type (dropdown)                             ?
? ?? Entity Type * (dropdown: Business/Individual)      ?
? ?? DBA - Doing Business As (text)                     ?
? ?? Effective Date (date)                              ?
?                                                         ?
? TAX & LICENSE INFORMATION                              ?
? ?? FEIN/SS # (text)                                   ?
? ?? License # (text)                                   ?
? ?? License State (text, max 2)                        ?
? ?? Date of Birth - for Individuals (date)             ?
?                                                         ?
? CONTACT INFORMATION                                    ?
? ?? Email (email)                                      ?
? ?? Business Phone (tel)                               ?
? ?? Mobile Phone (tel)                                 ?
?                                                         ?
? ADDRESS INFORMATION                                    ?
? ?? Street Address (text)                              ?
? ?? Apartment/Suite (text)                             ?
? ?? City (text)                                        ?
? ?? State (text, max 2)                                ?
? ?? Zip Code (text)                                    ?
? ?? Country (text, default: USA)                       ?
?                                                         ?
? TAX & PAYMENT INFORMATION ? RESTORED                 ?
? ?? ? 1099 Reportable (checkbox)                       ?
? ?? ? Subject to 1099 (checkbox)                       ?
? ?? ? Backup Withholding (checkbox)                    ?
? ?? ? Receives Bulk Payment (checkbox)                 ?
? ?? Payment Frequency (dropdown)                       ?
? ?   ?? Weekly                                         ?
? ?   ?? Bi-Weekly                                      ?
? ?   ?? Monthly                                        ?
? ?   ?? Quarterly                                      ?
? ?   ?? As Needed                                      ?
? ?? Bulk Payment Day/Date 1 (text: 1st, Monday, etc)  ?
? ?? Bulk Payment Day/Date 2 (text: 15th, Friday, etc) ?
?                                                         ?
? [Save Vendor] [Cancel]                               ?
?                                                         ?
???????????????????????????????????????????????????????????
```

---

## **Vendor List Table Columns**

```
? Vendor Name ? Party Type ? Entity Type ? Email ? Phone ? License # ? 1099 Subject ? Payment Freq ? Status ?
```

---

## **Database Fields Saved**

| Form Field | Database Column | Type | Required |
|-----------|-----------------|------|----------|
| Vendor Name | EntityName | NVARCHAR(500) | ? Yes |
| Party Type | PartyType | NVARCHAR(50) | ? Yes |
| Vendor Type | VendorType | NVARCHAR(50) | ? No |
| Entity Type | EntityType | CHAR(1) | ? Yes |
| DBA | DBA | NVARCHAR(200) | ? No |
| Effective Date | EntityEffectiveDate | DATETIME2 | ? No |
| FEIN/SS # | FEINorSS | NVARCHAR(50) | ? No |
| License # | LicenseNumber | NVARCHAR(50) | ? No |
| License State | LicenseState | NVARCHAR(2) | ? No |
| DOB | DateOfBirth | DATE | ? No |
| Email | Email | NVARCHAR(100) | ? No |
| Business Phone | HomeBusinessPhone | NVARCHAR(20) | ? No |
| Mobile Phone | MobilePhone | NVARCHAR(20) | ? No |
| Street Address | StreetAddress | NVARCHAR(200) | ? No |
| Apartment | Apt | NVARCHAR(50) | ? No |
| City | City | NVARCHAR(100) | ? No |
| State | State | NVARCHAR(2) | ? No |
| Zip Code | ZipCode | NVARCHAR(10) | ? No |
| Country | Country | NVARCHAR(50) | ? No |
| 1099 Reportable | Is1099Reportable | BIT | ? No |
| Subject to 1099 | IsSubjectTo1099 | BIT | ? No |
| Backup Withholding | IsBackupWithholding | BIT | ? No |
| Receives Bulk Payment | ReceivesBulkPayment | BIT | ? No |
| Payment Frequency | PaymentFrequency | NVARCHAR(20) | ? No |
| Bulk Payment Date 1 | BulkPaymentDayDate1 | NVARCHAR(20) | ? No |
| Bulk Payment Date 2 | BulkPaymentDayDate2 | NVARCHAR(20) | ? No |

---

## ? **Missing Fields Status**

| Field | Before | After |
|-------|--------|-------|
| Subject to 1099 | ? Missing | ? Restored |
| Backup Withholding | ? Missing | ? Restored |
| Payment Frequency | ? Missing | ? Restored |

---

**All fields now present and database-ready!** ??
