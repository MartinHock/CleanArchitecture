BEGIN TRANSACTION;
EXEC sp_rename N'Contributors', N'Contributors_old'

EXEC sp_rename N'Contributors_old.PK_Contributors', N'PK_Contributors_old', N'INDEX'

CREATE TABLE [Contributors] (
    [Id] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Status] int NOT NULL,
    [PhoneNumber_CountryCode] nvarchar(max) NULL,
    [PhoneNumber_Number] nvarchar(max) NULL,
    [PhoneNumber_Extension] nvarchar(max) NULL,
    CONSTRAINT [PK_Contributors] PRIMARY KEY ([Id])
);

INSERT INTO [Contributors] ([Id],[Name],[Status],[PhoneNumber_CountryCode],[PhoneNumber_Number],[PhoneNumber_Extension]) SELECT [Id],[Name],[Status],[PhoneNumber_CountryCode],[PhoneNumber_Number],[PhoneNumber_Extension] FROM [Contributors_old];

DROP TABLE [Contributors_old];

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260424000000_UseDbGeneratedIds';

COMMIT;
GO

