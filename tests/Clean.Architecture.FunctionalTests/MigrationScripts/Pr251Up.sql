BEGIN TRANSACTION;
EXEC sp_rename N'Contributors', N'Contributors_old'

EXEC sp_rename N'Contributors_old.PK_Contributors', N'PK_Contributors_old', N'INDEX'

CREATE TABLE [Contributors] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Status] int NOT NULL,
    [PhoneNumber_CountryCode] nvarchar(max) NULL,
    [PhoneNumber_Number] nvarchar(max) NULL,
    [PhoneNumber_Extension] nvarchar(max) NULL,
    CONSTRAINT [PK_Contributors] PRIMARY KEY ([Id])
);

SET IDENTITY_INSERT [Contributors] ON;INSERT INTO [Contributors] ([Id],[Name],[Status],[PhoneNumber_CountryCode],[PhoneNumber_Number],[PhoneNumber_Extension]) SELECT [Id],[Name],[Status],[PhoneNumber_CountryCode],[PhoneNumber_Number],[PhoneNumber_Extension] FROM [Contributors_old];SET IDENTITY_INSERT [Contributors] OFF;

DROP TABLE [Contributors_old];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424000000_UseDbGeneratedIds', N'11.0.0');

COMMIT;
GO

