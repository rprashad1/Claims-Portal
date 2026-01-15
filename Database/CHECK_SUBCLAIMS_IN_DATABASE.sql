-- CHECK_SUBCLAIMS_IN_DATABASE.sql
-- Run this script to diagnose why sub-claims are not displaying

-- 1. Check if any sub-claims exist in the database
SELECT 'Total SubClaims in Database' AS [Check], COUNT(*) AS [Count]
FROM SubClaims;

-- 2. List all claims with their sub-claim counts
SELECT 
    f.ClaimNumber,
    f.FnolNumber,
    f.FnolStatus,
    f.CreatedDate,
    COUNT(sc.SubClaimId) AS SubClaimCount
FROM FNOL f
LEFT JOIN SubClaims sc ON f.FnolId = sc.FnolId
WHERE f.ClaimNumber IS NOT NULL AND f.ClaimNumber <> ''
GROUP BY f.ClaimNumber, f.FnolNumber, f.FnolStatus, f.CreatedDate
ORDER BY f.CreatedDate DESC;

-- 3. Show all sub-claims with their details
SELECT 
    sc.SubClaimId,
    sc.FnolId,
    sc.ClaimNumber,
    sc.SubClaimNumber,
    sc.FeatureNumber,
    sc.ClaimantName,
    sc.Coverage,
    sc.SubClaimStatus,
    sc.CreatedDate
FROM SubClaims sc
ORDER BY sc.CreatedDate DESC;

-- 4. Check FNOL table for FnolId values
SELECT 
    FnolId,
    FnolNumber,
    ClaimNumber,
    FnolStatus,
    CreatedDate
FROM FNOL
WHERE ClaimNumber IS NOT NULL AND ClaimNumber <> ''
ORDER BY CreatedDate DESC;
