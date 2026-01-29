-- Update LetterGen_AdminRules.Claimant values to a normalized 'InsuredDriver' variant
-- Run this in the ClaimsPortal database. Review and backup before running in production.

BEGIN TRANSACTION;

-- Option A: Update a specific rule by Id (replace <RULE_ID> with the rule's Id)
-- UPDATE dbo.LetterGen_AdminRules SET Claimant = 'InsuredDriver' WHERE Id = <RULE_ID>;

-- Option B: Heuristic update: any claimant containing 'Insured' and 'Driver' -> 'InsuredDriver'
UPDATE dbo.LetterGen_AdminRules
SET Claimant = 'InsuredDriver'
WHERE Claimant IS NOT NULL
  AND Claimant LIKE '%Insured%'
  AND Claimant LIKE '%Driver%';

-- Option C: If you prefer 'Insured Vehicle Driver' -> 'InsuredDriver'
UPDATE dbo.LetterGen_AdminRules
SET Claimant = 'InsuredDriver'
WHERE Claimant = 'Insured Vehicle Driver';

COMMIT TRANSACTION;

PRINT 'Updated LetterGen_AdminRules Claimant values to InsuredDriver where applicable.';
