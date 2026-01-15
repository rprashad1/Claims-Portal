-- ============================================================================
-- FIX: ClaimNumber UNIQUE Constraint and FnolStatus CHECK Constraint
-- ============================================================================
-- Problem 1: The current UNIQUE constraint on ClaimNumber doesn't allow 
-- multiple NULL values, which prevents creating multiple draft FNOLs.
--
-- Problem 2: The CK_FnolStatus check constraint only allows 'O' and 'C'
-- but the application uses 'D' for Draft status.
--
-- Solution: Fix both constraints.
-- ============================================================================

-- ============================================================================
-- FIX 1: FnolStatus CHECK Constraint - Add 'D' for Draft
-- ============================================================================

-- Drop the existing check constraint
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_FnolStatus' AND parent_object_id = OBJECT_ID('FNOL'))
BEGIN
    ALTER TABLE FNOL DROP CONSTRAINT CK_FnolStatus;
    PRINT 'Dropped constraint: CK_FnolStatus';
END

GO

-- Create new check constraint that includes 'D' for Draft
ALTER TABLE FNOL ADD CONSTRAINT CK_FnolStatus CHECK (FnolStatus IN ('O', 'C', 'D'));
PRINT 'Created new constraint CK_FnolStatus allowing O, C, and D';

GO

-- ============================================================================
-- FIX 2: ClaimNumber UNIQUE Constraint - Allow Multiple NULLs
-- ============================================================================

DECLARE @ConstraintName NVARCHAR(200);

-- Find unique constraint on ClaimNumber
SELECT @ConstraintName = name
FROM sys.key_constraints
WHERE parent_object_id = OBJECT_ID('FNOL')
  AND type = 'UQ'
  AND EXISTS (
    SELECT 1 FROM sys.index_columns ic
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE ic.object_id = parent_object_id 
      AND ic.index_id = unique_index_id
      AND c.name = 'ClaimNumber'
  );

-- Drop the constraint if found
IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE FNOL DROP CONSTRAINT ' + @ConstraintName);
    PRINT 'Dropped constraint: ' + @ConstraintName;
END
ELSE
BEGIN
    PRINT 'No unique constraint found on ClaimNumber column';
END

GO

-- Check for and drop any unique index on ClaimNumber
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FNOL') AND name = 'IX_FNOL_ClaimNumber_Unique')
BEGIN
    DROP INDEX IX_FNOL_ClaimNumber_Unique ON FNOL;
    PRINT 'Dropped index: IX_FNOL_ClaimNumber_Unique';
END

GO

-- Create a filtered unique index that only applies to non-NULL values
-- This allows multiple NULL values while still enforcing uniqueness on actual claim numbers
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FNOL') AND name = 'IX_FNOL_ClaimNumber_Unique_NonNull')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_FNOL_ClaimNumber_Unique_NonNull
    ON FNOL(ClaimNumber)
    WHERE ClaimNumber IS NOT NULL;
    
    PRINT 'Created filtered unique index: IX_FNOL_ClaimNumber_Unique_NonNull';
END

GO

-- ============================================================================
-- SUMMARY
-- ============================================================================

PRINT '';
PRINT '============================================';
PRINT 'Database fix completed successfully!';
PRINT '============================================';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. Updated CK_FnolStatus to allow D (Draft) in addition to O (Open) and C (Closed)';
PRINT '2. Removed UNIQUE constraint on ClaimNumber that prevented multiple NULLs';
PRINT '3. Created filtered unique index that allows multiple NULL ClaimNumbers';
PRINT '4. ClaimNumber uniqueness is still enforced for non-NULL values';
PRINT '';
PRINT 'Valid FnolStatus values are now:';
PRINT '  O = Open';
PRINT '  C = Closed/Converted to Claim';
PRINT '  D = Draft';

GO
