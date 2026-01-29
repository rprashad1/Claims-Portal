-- SQL scripts to add a uniqueness constraint/index to prevent duplicate rules
-- File: sql/add_unique_constraint_lettergen_adminrules.sql
-- Two options provided. Pick one to run depending on desired behavior.

-- 0) Safety check: list any existing duplicate rule groups
--    (Coverage, Claimant, IsAttorneyRepresented, DocumentName) and the count
SELECT Coverage, Claimant, IsAttorneyRepresented, DocumentName, COUNT(*) AS Cnt
FROM dbo.LetterGenAdminRules
GROUP BY Coverage, Claimant, IsAttorneyRepresented, DocumentName
HAVING COUNT(*) > 1;

-- If the above returns rows, you must resolve duplicates before creating a unique index.

-- OPTION A: Create a filtered unique index that only enforces uniqueness for ACTIVE rules.
-- This allows you to have duplicates for inactive rules but prevents multiple active rules with the same key.
-- Good when you want to preserve historical/inactive rows.

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.tables t ON i.object_id = t.object_id
    WHERE t.name = 'LetterGenAdminRules' AND i.name = 'UX_LG_AdminRules_Active_Key'
)
BEGIN
    CREATE UNIQUE INDEX UX_LG_AdminRules_Active_Key
    ON dbo.LetterGenAdminRules (Coverage, Claimant, IsAttorneyRepresented, DocumentName)
    WHERE IsActive = 1;
END
GO

-- OPTION B: Create a full unique constraint (prevents duplicates regardless of active/inactive)
-- Only use this when you want absolute uniqueness across all rows.

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.tables t ON i.object_id = t.object_id
    WHERE t.name = 'LetterGenAdminRules' AND i.name = 'UX_LG_AdminRules_Full_Key'
)
BEGIN
    CREATE UNIQUE INDEX UX_LG_AdminRules_Full_Key
    ON dbo.LetterGenAdminRules (Coverage, Claimant, IsAttorneyRepresented, DocumentName);
END
GO

-- Helpful cleanup suggestions (manual review recommended):
-- 1) If duplicates exist and you want Option A, you can mark older duplicates as inactive.
-- 2) If duplicates exist and you want Option B, decide which rows to keep and delete or consolidate the others.

-- Example to deactivate duplicates keeping the MIN(Id) per duplicate group (use cautiously!):
-- BEGIN TRANSACTION;
-- WITH Duplicates AS (
--   SELECT Id,
--          ROW_NUMBER() OVER (PARTITION BY Coverage, Claimant, IsAttorneyRepresented, DocumentName ORDER BY Id) rn
--   FROM dbo.LetterGen_AdminRules
-- )
-- UPDATE dbo.LetterGen_AdminRules
-- SET IsActive = 0
-- FROM dbo.LetterGen_AdminRules a
-- JOIN Duplicates d ON a.Id = d.Id
-- WHERE d.rn > 1;
-- COMMIT TRANSACTION;

-- After duplicates are resolved, rerun the index creation (Option A or B) as needed.
