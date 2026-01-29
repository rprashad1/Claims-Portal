-- Safe cleanup script for duplicate LetterGen admin rules
-- File: sql/cleanup_deactivate_duplicates_lettergen_adminrules.sql
-- Purpose: identify duplicate rules by (Coverage, Claimant, IsAttorneyRepresented, DocumentName)
-- and optionally deactivate duplicates while keeping the row with the lowest Id.
--
-- Instructions:
-- 1) Run this script as-is to review the rows that would be deactivated. It will NOT update by default.
-- 2) If output looks correct, set @DoUpdate = 1 and re-run to apply the changes.
-- 3) Always backup your database before running destructive changes.

SET NOCOUNT ON;

DECLARE @DoUpdate BIT = 0; -- Set to 1 to actually deactivate duplicates

-- 1) Show duplicate groups (for review)
SELECT Coverage, Claimant, IsAttorneyRepresented, DocumentName, COUNT(*) AS DuplicateCount
FROM dbo.LetterGenAdminRules
GROUP BY Coverage, Claimant, IsAttorneyRepresented, DocumentName
HAVING COUNT(*) > 1;

-- 2) Build list of duplicate Ids to deactivate (keep the lowest Id per group)
IF OBJECT_ID('tempdb..#ToDeactivate') IS NOT NULL
    DROP TABLE #ToDeactivate;

CREATE TABLE #ToDeactivate (Id BIGINT NOT NULL PRIMARY KEY);

INSERT INTO #ToDeactivate (Id)
SELECT Id
FROM (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY Coverage, Claimant, IsAttorneyRepresented, DocumentName ORDER BY Id) rn
    FROM dbo.LetterGenAdminRules
) t
WHERE t.rn > 1;

-- 3) Show candidate rows for deactivation (review)
SELECT a.*
FROM dbo.LetterGenAdminRules a
JOIN #ToDeactivate d ON a.Id = d.Id
ORDER BY a.Coverage, a.Claimant, a.DocumentName, a.Id;

-- 4) If user confirms, perform the update to set IsActive = 0 on duplicates
IF @DoUpdate = 1
BEGIN
    PRINT 'Applying deactivation to duplicates...';
    BEGIN TRANSACTION;
    UPDATE dbo.LetterGenAdminRules
    SET IsActive = 0,
        UpdatedAt = SYSUTCDATETIME(),
        UpdatedBy = SUSER_SNAME()
    FROM dbo.LetterGenAdminRules a
    INNER JOIN #ToDeactivate d ON a.Id = d.Id;

    PRINT 'Rows deactivated:';
    SELECT @@ROWCOUNT as RowsAffected;

    COMMIT TRANSACTION;
END
ELSE
BEGIN
    PRINT 'Dry run complete. To apply changes set @DoUpdate = 1 and rerun the script.';
END

-- Cleanup temp
DROP TABLE IF EXISTS #ToDeactivate;

GO
