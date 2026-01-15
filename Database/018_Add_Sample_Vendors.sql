-- ============================================================================
-- ADD SAMPLE VENDORS FOR HOSPITALS, ATTORNEYS, AND AUTHORITIES
-- ============================================================================
-- Purpose: Populate EntityMaster with sample vendors for testing the 
-- vendor search functionality for Hospitals, Attorneys, and Authorities
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding Sample Vendors (Hospitals, Attorneys, Authorities) ===';
PRINT '';

-- ============================================================================
-- HOSPITALS
-- ============================================================================
PRINT 'Adding Hospitals...';

-- Hospital 1
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Memorial General Hospital' AND PartyType = 'Hospital')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Hospital', 'Vendor', 'Hospital', 'Memorial General Hospital', NULL,
        '(555) 100-1001', 'info@memorialhospital.com', 'Y', GETDATE(), 'system');
    
    DECLARE @HospitalId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@HospitalId1, 'M', '100 Hospital Drive', 'Springfield', 'IL', '62701', 'Y', GETDATE(), 'system');
    PRINT '  Added: Memorial General Hospital';
END

-- Hospital 2
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'St. Mary Medical Center' AND PartyType = 'Hospital')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Hospital', 'Vendor', 'Hospital', 'St. Mary Medical Center', NULL,
        '(555) 100-1002', 'info@stmarymedical.com', 'Y', GETDATE(), 'system');
    
    DECLARE @HospitalId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@HospitalId2, 'M', '200 Medical Center Blvd', 'Chicago', 'IL', '60601', 'Y', GETDATE(), 'system');
    PRINT '  Added: St. Mary Medical Center';
END

-- Hospital 3
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'City Regional Hospital' AND PartyType = 'Hospital')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Hospital', 'Vendor', 'Hospital', 'City Regional Hospital', NULL,
        '(555) 100-1003', 'info@cityregional.com', 'Y', GETDATE(), 'system');
    
    DECLARE @HospitalId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@HospitalId3, 'M', '500 Regional Way', 'Aurora', 'IL', '60504', 'Y', GETDATE(), 'system');
    PRINT '  Added: City Regional Hospital';
END

-- Hospital 4
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Mercy Health System' AND PartyType = 'Hospital')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Hospital', 'Vendor', 'Hospital', 'Mercy Health System', 'Mercy Hospital',
        '(555) 100-1004', 'info@mercyhealth.com', 'Y', GETDATE(), 'system');
    
    DECLARE @HospitalId4 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@HospitalId4, 'M', '750 Mercy Lane', 'Naperville', 'IL', '60540', 'Y', GETDATE(), 'system');
    PRINT '  Added: Mercy Health System';
END

-- Hospital 5
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Northwestern Memorial Hospital' AND PartyType = 'Hospital')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Hospital', 'Vendor', 'Hospital', 'Northwestern Memorial Hospital', NULL,
        '(312) 926-2000', 'info@nm.org', 'Y', GETDATE(), 'system');
    
    DECLARE @HospitalId5 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@HospitalId5, 'M', '251 E Huron St', 'Chicago', 'IL', '60611', 'Y', GETDATE(), 'system');
    PRINT '  Added: Northwestern Memorial Hospital';
END

-- ============================================================================
-- DEFENSE ATTORNEYS
-- ============================================================================
PRINT '';
PRINT 'Adding Defense Attorneys...';

-- Defense Attorney 1
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Smith & Associates Law Firm' AND PartyType = 'Defense Attorney')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Defense Attorney', 'Vendor', 'Defense Attorney', 'Smith & Associates Law Firm', NULL, 'John Smith, Esq.',
        '(555) 200-2001', 'jsmith@smithlaw.com', 'Y', GETDATE(), 'system');
    
    DECLARE @DefAttorneyId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@DefAttorneyId1, 'M', '100 Legal Plaza, Suite 500', 'Chicago', 'IL', '60601', 'Y', GETDATE(), 'system');
    PRINT '  Added: Smith & Associates Law Firm';
END

-- Defense Attorney 2
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Johnson Defense Group' AND PartyType = 'Defense Attorney')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Defense Attorney', 'Vendor', 'Defense Attorney', 'Johnson Defense Group', NULL, 'Mary Johnson, Esq.',
        '(555) 200-2002', 'mjohnson@johnsondefense.com', 'Y', GETDATE(), 'system');
    
    DECLARE @DefAttorneyId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@DefAttorneyId2, 'M', '200 Corporate Center, Floor 10', 'Springfield', 'IL', '62701', 'Y', GETDATE(), 'system');
    PRINT '  Added: Johnson Defense Group';
END

-- Defense Attorney 3
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Williams & Burke LLC' AND PartyType = 'Defense Attorney')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Defense Attorney', 'Vendor', 'Defense Attorney', 'Williams & Burke LLC', NULL, 'Robert Williams, Esq.',
        '(555) 200-2003', 'rwilliams@wblaw.com', 'Y', GETDATE(), 'system');
    
    DECLARE @DefAttorneyId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@DefAttorneyId3, 'M', '300 Tower Place, Suite 1200', 'Aurora', 'IL', '60504', 'Y', GETDATE(), 'system');
    PRINT '  Added: Williams & Burke LLC';
END

-- ============================================================================
-- PLAINTIFF ATTORNEYS
-- ============================================================================
PRINT '';
PRINT 'Adding Plaintiff Attorneys...';

-- Plaintiff Attorney 1
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Martinez Injury Attorneys' AND PartyType = 'Plaintiff Attorneys')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Plaintiff Attorneys', 'Vendor', 'Plaintiff Attorneys', 'Martinez Injury Attorneys', 'Martinez Law', 'Carlos Martinez, Esq.',
        '(555) 300-3001', 'cmartinez@martinezlaw.com', 'Y', GETDATE(), 'system');
    
    DECLARE @PlaintiffAttorneyId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PlaintiffAttorneyId1, 'M', '400 Justice Drive', 'Chicago', 'IL', '60602', 'Y', GETDATE(), 'system');
    PRINT '  Added: Martinez Injury Attorneys';
END

-- Plaintiff Attorney 2
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Thompson Personal Injury Law' AND PartyType = 'Plaintiff Attorneys')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Plaintiff Attorneys', 'Vendor', 'Plaintiff Attorneys', 'Thompson Personal Injury Law', NULL, 'Sarah Thompson, Esq.',
        '(555) 300-3002', 'sthompson@thompsonpi.com', 'Y', GETDATE(), 'system');
    
    DECLARE @PlaintiffAttorneyId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PlaintiffAttorneyId2, 'M', '500 Advocate Way, Suite 300', 'Naperville', 'IL', '60540', 'Y', GETDATE(), 'system');
    PRINT '  Added: Thompson Personal Injury Law';
END

-- Plaintiff Attorney 3
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Davis & Associates' AND PartyType = 'Plaintiff Attorneys')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, ContactName,
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Plaintiff Attorneys', 'Vendor', 'Plaintiff Attorneys', 'Davis & Associates', NULL, 'Michael Davis, Esq.',
        '(555) 300-3003', 'mdavis@davislaw.com', 'Y', GETDATE(), 'system');
    
    DECLARE @PlaintiffAttorneyId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PlaintiffAttorneyId3, 'M', '600 Client First Blvd', 'Springfield', 'IL', '62702', 'Y', GETDATE(), 'system');
    PRINT '  Added: Davis & Associates';
END

-- ============================================================================
-- POLICE DEPARTMENTS
-- ============================================================================
PRINT '';
PRINT 'Adding Police Departments...';

-- Police Department 1
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Springfield Police Department' AND PartyType = 'Police Department')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Police Department', 'Vendor', 'Police Department', 'Springfield Police Department', NULL,
        '(555) 400-4001', 'records@springfieldpd.gov', 'Y', GETDATE(), 'system');
    
    DECLARE @PoliceId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PoliceId1, 'M', '800 Civic Center Drive', 'Springfield', 'IL', '62701', 'Y', GETDATE(), 'system');
    PRINT '  Added: Springfield Police Department';
END

-- Police Department 2
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Chicago Police Department' AND PartyType = 'Police Department')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Police Department', 'Vendor', 'Police Department', 'Chicago Police Department', 'CPD',
        '(312) 746-6000', 'records@chicagopolice.org', 'Y', GETDATE(), 'system');
    
    DECLARE @PoliceId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PoliceId2, 'M', '3510 S Michigan Ave', 'Chicago', 'IL', '60653', 'Y', GETDATE(), 'system');
    PRINT '  Added: Chicago Police Department';
END

-- Police Department 3
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Aurora Police Department' AND PartyType = 'Police Department')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Police Department', 'Vendor', 'Police Department', 'Aurora Police Department', NULL,
        '(630) 256-5000', 'records@aurora-il.org', 'Y', GETDATE(), 'system');
    
    DECLARE @PoliceId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PoliceId3, 'M', '1200 E Indian Trail', 'Aurora', 'IL', '60505', 'Y', GETDATE(), 'system');
    PRINT '  Added: Aurora Police Department';
END

-- Police Department 4
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Illinois State Police' AND PartyType = 'Police Department')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Police Department', 'Vendor', 'Police Department', 'Illinois State Police', 'ISP',
        '(217) 782-6637', 'records@isp.state.il.us', 'Y', GETDATE(), 'system');
    
    DECLARE @PoliceId4 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@PoliceId4, 'M', '801 S 7th St', 'Springfield', 'IL', '62703', 'Y', GETDATE(), 'system');
    PRINT '  Added: Illinois State Police';
END

-- ============================================================================
-- FIRE STATIONS
-- ============================================================================
PRINT '';
PRINT 'Adding Fire Stations...';

-- Fire Station 1
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Springfield Fire Department' AND PartyType = 'Fire Station')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Fire Station', 'Vendor', 'Fire Station', 'Springfield Fire Department', NULL,
        '(555) 500-5001', 'records@springfieldfire.gov', 'Y', GETDATE(), 'system');
    
    DECLARE @FireId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@FireId1, 'M', '900 Fire Station Way', 'Springfield', 'IL', '62701', 'Y', GETDATE(), 'system');
    PRINT '  Added: Springfield Fire Department';
END

-- Fire Station 2
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Chicago Fire Department' AND PartyType = 'Fire Station')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Fire Station', 'Vendor', 'Fire Station', 'Chicago Fire Department', 'CFD',
        '(312) 744-4730', 'records@chicagofire.org', 'Y', GETDATE(), 'system');
    
    DECLARE @FireId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@FireId2, 'M', '444 N Dearborn St', 'Chicago', 'IL', '60654', 'Y', GETDATE(), 'system');
    PRINT '  Added: Chicago Fire Department';
END

-- Fire Station 3
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE EntityName = 'Aurora Fire Department' AND PartyType = 'Fire Station')
BEGIN
    INSERT INTO EntityMaster (EntityType, PartyType, EntityGroupCode, VendorType, EntityName, DBA, 
        HomeBusinessPhone, Email, EntityStatus, CreatedDate, CreatedBy)
    VALUES ('B', 'Fire Station', 'Vendor', 'Fire Station', 'Aurora Fire Department', NULL,
        '(630) 256-4600', 'records@aurorafire.org', 'Y', GETDATE(), 'system');
    
    DECLARE @FireId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO AddressMaster (EntityId, AddressType, StreetAddress, City, State, ZipCode, AddressStatus, CreatedDate, CreatedBy)
    VALUES (@FireId3, 'M', '75 N Broadway', 'Aurora', 'IL', '60505', 'Y', GETDATE(), 'system');
    PRINT '  Added: Aurora Fire Department';
END

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: Sample Vendors Added ===';
PRINT '';

SELECT 
    'Hospitals' AS VendorCategory,
    COUNT(*) AS Count
FROM EntityMaster 
WHERE PartyType = 'Hospital' AND EntityStatus = 'Y'
UNION ALL
SELECT 
    'Defense Attorneys',
    COUNT(*)
FROM EntityMaster 
WHERE PartyType = 'Defense Attorney' AND EntityStatus = 'Y'
UNION ALL
SELECT 
    'Plaintiff Attorneys',
    COUNT(*)
FROM EntityMaster 
WHERE PartyType = 'Plaintiff Attorneys' AND EntityStatus = 'Y'
UNION ALL
SELECT 
    'Police Departments',
    COUNT(*)
FROM EntityMaster 
WHERE PartyType = 'Police Department' AND EntityStatus = 'Y'
UNION ALL
SELECT 
    'Fire Stations',
    COUNT(*)
FROM EntityMaster 
WHERE PartyType = 'Fire Station' AND EntityStatus = 'Y';
GO

PRINT '';
PRINT '================================================';
PRINT 'Sample Vendors Added Successfully';
PRINT '================================================';
PRINT '';
PRINT 'Categories added:';
PRINT '  - Hospitals (5 records)';
PRINT '  - Defense Attorneys (3 records)';
PRINT '  - Plaintiff Attorneys (3 records)';
PRINT '  - Police Departments (4 records)';
PRINT '  - Fire Stations (3 records)';
PRINT '';
PRINT 'These vendors can now be searched from:';
PRINT '  - Hospital Search (Injury Template)';
PRINT '  - Attorney Search Modal';
PRINT '  - Authority Modal';
PRINT '================================================';
GO
