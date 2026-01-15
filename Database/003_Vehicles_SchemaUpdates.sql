-- ============================================================================
-- VEHICLES AND FNOL TABLE SCHEMA UPDATES
-- Run this script AFTER 001_InitialSchema.sql
-- ============================================================================

-- ============================================================================
-- FIX: Remove FK_FNOL_Policy constraint (Policy comes from external system)
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FNOL_Policy')
BEGIN
    ALTER TABLE FNOL DROP CONSTRAINT FK_FNOL_Policy;
    PRINT 'Removed FK_FNOL_Policy constraint - Policy lookup is from external system.';
END
GO

-- ============================================================================
-- VEHICLES TABLE UPDATES
-- ============================================================================

-- CHANGE 1: Add HasDashCam column (Dash Cam Installed - Yes/No)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'HasDashCam')
BEGIN
    ALTER TABLE Vehicles
    ADD HasDashCam BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasDashCam column to Vehicles table.';
END
GO

-- CHANGE 2: Add DidVehicleRollOver column (Vehicle Did Roll Over - Yes/No)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'DidVehicleRollOver')
BEGIN
    ALTER TABLE Vehicles
    ADD DidVehicleRollOver BIT NOT NULL DEFAULT 0;
    PRINT 'Added DidVehicleRollOver column to Vehicles table.';
END
GO

-- CHANGE 3: Add PlateState column (License Plate State)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'PlateState')
BEGIN
    ALTER TABLE Vehicles
    ADD PlateState NVARCHAR(2) NULL;
    PRINT 'Added PlateState column to Vehicles table.';
END
GO

-- ============================================================================
-- FNOL TABLE UPDATES
-- ============================================================================

-- CHANGE 4: Add LossLocation2 column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'LossLocation2')
BEGIN
    ALTER TABLE FNOL
    ADD LossLocation2 NVARCHAR(500) NULL;
    PRINT 'Added LossLocation2 column to FNOL table.';
END
GO

-- CHANGE 5: Add HasOtherVehiclesInvolved column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'HasOtherVehiclesInvolved')
BEGIN
    ALTER TABLE FNOL
    ADD HasOtherVehiclesInvolved BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasOtherVehiclesInvolved column to FNOL table.';
END
GO

-- CHANGE 6: Add ReportedBy column (Insured, Agent, Other)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedBy')
BEGIN
    ALTER TABLE FNOL
    ADD ReportedBy NVARCHAR(50) NULL;
    PRINT 'Added ReportedBy column to FNOL table.';
END
GO

-- CHANGE 7: Add ReportedByName column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByName')
BEGIN
    ALTER TABLE FNOL
    ADD ReportedByName NVARCHAR(200) NULL;
    PRINT 'Added ReportedByName column to FNOL table.';
END
GO

-- CHANGE 8: Add ReportedByPhone column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByPhone')
BEGIN
    ALTER TABLE FNOL
    ADD ReportedByPhone NVARCHAR(20) NULL;
    PRINT 'Added ReportedByPhone column to FNOL table.';
END
GO

-- CHANGE 9: Add ReportedByEmail column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByEmail')
BEGIN
    ALTER TABLE FNOL
    ADD ReportedByEmail NVARCHAR(100) NULL;
    PRINT 'Added ReportedByEmail column to FNOL table.';
END
GO

-- CHANGE 10: Add ReportingMethod column (Phone, Email, Web, Fax)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportingMethod')
BEGIN
    ALTER TABLE FNOL
    ADD ReportingMethod NVARCHAR(50) NULL;
    PRINT 'Added ReportingMethod column to FNOL table.';
END
GO

-- CHANGE 11: Add ReportedByEntityId column (FK to EntityMaster)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'ReportedByEntityId')
BEGIN
    ALTER TABLE FNOL
    ADD ReportedByEntityId BIGINT NULL;
    PRINT 'Added ReportedByEntityId column to FNOL table.';
END
GO

-- ============================================================================
-- CLAIMANTS TABLE UPDATES - Add Attorney Reference
-- ============================================================================

-- CHANGE 12: Ensure AttorneyEntityId exists in Claimants
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claimants') AND name = 'AttorneyEntityId')
BEGIN
    ALTER TABLE Claimants
    ADD AttorneyEntityId BIGINT NULL;
    PRINT 'Added AttorneyEntityId column to Claimants table.';
END
GO

-- ============================================================================
-- FNOL WITNESSES TABLE - New table for witnesses
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FnolWitnesses')
BEGIN
    CREATE TABLE FnolWitnesses (
        FnolWitnessId BIGINT IDENTITY(1,1) PRIMARY KEY,
        FnolId BIGINT NOT NULL,
        EntityId BIGINT NOT NULL,                    -- FK to EntityMaster
        WitnessStatement NVARCHAR(MAX) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        
        CONSTRAINT FK_FnolWitnesses_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
        CONSTRAINT FK_FnolWitnesses_Entity FOREIGN KEY (EntityId) REFERENCES EntityMaster(EntityId)
    );
    
    CREATE INDEX IX_FnolWitnesses_FnolId ON FnolWitnesses(FnolId);
    PRINT 'Created FnolWitnesses table.';
END
GO

-- ============================================================================
-- FNOL AUTHORITIES TABLE - New table for authorities notified
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FnolAuthorities')
BEGIN
    CREATE TABLE FnolAuthorities (
        FnolAuthorityId BIGINT IDENTITY(1,1) PRIMARY KEY,
        FnolId BIGINT NOT NULL,
        EntityId BIGINT NOT NULL,                    -- FK to EntityMaster (Vendor)
        AuthorityType NVARCHAR(50) NULL,             -- Police Department, Fire Station
        ReportNumber NVARCHAR(100) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        
        CONSTRAINT FK_FnolAuthorities_FNOL FOREIGN KEY (FnolId) REFERENCES FNOL(FnolId),
        CONSTRAINT FK_FnolAuthorities_Entity FOREIGN KEY (EntityId) REFERENCES EntityMaster(EntityId)
    );
    
    CREATE INDEX IX_FnolAuthorities_FnolId ON FnolAuthorities(FnolId);
    PRINT 'Created FnolAuthorities table.';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================

PRINT '';
PRINT '============================================';
PRINT 'VERIFICATION: FNOL table columns';
PRINT '============================================';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FNOL'
ORDER BY ORDINAL_POSITION;

GO

PRINT '';
PRINT '============================================';
PRINT 'VERIFICATION: Foreign Keys on FNOL table';
PRINT '============================================';

SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'FNOL';

GO

PRINT '';
PRINT '============================================';
PRINT 'ALL SCHEMA UPDATES COMPLETED SUCCESSFULLY!';
PRINT '============================================';
