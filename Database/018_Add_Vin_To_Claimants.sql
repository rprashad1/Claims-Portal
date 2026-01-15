-- Add Vin column to Claimants to store vehicle VIN reference for passenger claimants
IF NOT EXISTS(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Claimants') AND name = 'Vin'
)
BEGIN
    ALTER TABLE Claimants ADD Vin NVARCHAR(17) NULL;
    PRINT 'Added Vin column to Claimants';
END
ELSE
BEGIN
    PRINT 'Column Vin already exists on Claimants';
END

-- Optional index to speed lookups by VIN
IF NOT EXISTS(
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Claimants_Vin' AND object_id = OBJECT_ID('Claimants')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Claimants_Vin ON Claimants(Vin) WHERE Vin IS NOT NULL;
    PRINT 'Created index IX_Claimants_Vin';
END
ELSE
BEGIN
    PRINT 'Index IX_Claimants_Vin already exists';
END
