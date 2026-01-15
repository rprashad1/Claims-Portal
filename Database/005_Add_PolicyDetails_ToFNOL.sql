-- ============================================================================
-- ADD POLICY DETAILS COLUMNS TO FNOL TABLE
-- ============================================================================
-- Purpose: Store additional policy information when policy is verified
-- so that the FNOL draft can be loaded with complete policy details.
-- ============================================================================

-- Add Insured Name column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredName')
BEGIN
    ALTER TABLE FNOL ADD InsuredName NVARCHAR(200) NULL;
    PRINT 'Added column: InsuredName';
END
GO

-- Add Insured Phone column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredPhone')
BEGIN
    ALTER TABLE FNOL ADD InsuredPhone NVARCHAR(20) NULL;
    PRINT 'Added column: InsuredPhone';
END
GO

-- Add Renewal Number column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'RenewalNumber')
BEGIN
    ALTER TABLE FNOL ADD RenewalNumber INT NULL;
    PRINT 'Added column: RenewalNumber';
END
GO

-- Add LossLocation2 column (if not exists from previous migration)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'LossLocation2')
BEGIN
    ALTER TABLE FNOL ADD LossLocation2 NVARCHAR(MAX) NULL;
    PRINT 'Added column: LossLocation2';
END
GO

-- Add HasOtherVehiclesInvolved column (if not exists)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'HasOtherVehiclesInvolved')
BEGIN
    ALTER TABLE FNOL ADD HasOtherVehiclesInvolved BIT DEFAULT 0;
    PRINT 'Added column: HasOtherVehiclesInvolved';
END
GO

-- Add ReportedBy columns (if not exists)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedBy')
BEGIN
    ALTER TABLE FNOL ADD ReportedBy NVARCHAR(50) NULL;
    PRINT 'Added column: ReportedBy';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByName')
BEGIN
    ALTER TABLE FNOL ADD ReportedByName NVARCHAR(200) NULL;
    PRINT 'Added column: ReportedByName';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByPhone')
BEGIN
    ALTER TABLE FNOL ADD ReportedByPhone NVARCHAR(20) NULL;
    PRINT 'Added column: ReportedByPhone';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByEmail')
BEGIN
    ALTER TABLE FNOL ADD ReportedByEmail NVARCHAR(100) NULL;
    PRINT 'Added column: ReportedByEmail';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportingMethod')
BEGIN
    ALTER TABLE FNOL ADD ReportingMethod NVARCHAR(50) NULL;
    PRINT 'Added column: ReportingMethod';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByEntityId')
BEGIN
    ALTER TABLE FNOL ADD ReportedByEntityId BIGINT NULL;
    PRINT 'Added column: ReportedByEntityId';
END
GO

-- ============================================================================
-- SUMMARY
-- ============================================================================
PRINT '';
PRINT '============================================';
PRINT 'Policy Details columns added to FNOL table';
PRINT '============================================';
PRINT 'New columns:';
PRINT '  - InsuredName (NVARCHAR 200)';
PRINT '  - InsuredPhone (NVARCHAR 20)';
PRINT '  - RenewalNumber (INT)';
PRINT '';
PRINT 'These columns store policy information when';
PRINT 'a policy is verified during FNOL creation.';
PRINT '============================================';
GO
