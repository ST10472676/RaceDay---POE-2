/* Run once against the existing RaceDayDB database after backing it up. */
IF COL_LENGTH('dbo.Users', 'ProfilePictureUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfilePictureUrl NVARCHAR(500) NULL;
GO
