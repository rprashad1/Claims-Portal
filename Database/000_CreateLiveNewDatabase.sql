-- ============================================================================
-- CLAIMSPORTAL DATABASE SETUP SCRIPT
-- Run this on: HICD09062024\SQLEXPRESS
-- Database Name: ClaimsPortal (renamed from LiveNew)
-- ============================================================================

-- Step 1: Create the ClaimsPortal database (if it doesn't exist)
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'ClaimsPortal')
BEGIN
    CREATE DATABASE ClaimsPortal;
    PRINT 'Database ClaimsPortal created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ClaimsPortal already exists.';
END;

GO

-- Step 2: Use the ClaimsPortal database
USE ClaimsPortal;
GO

-- Step 3: Verify database is ready
PRINT 'Database is ready for schema creation.';
PRINT 'Server: ' + @@SERVERNAME;
PRINT 'Database: ' + DB_NAME();
PRINT 'Current Time: ' + CONVERT(NVARCHAR(30), GETDATE(), 121);

