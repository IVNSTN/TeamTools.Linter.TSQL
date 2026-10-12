INSERT dbo.foo VALUES (1);

SET @id = SCOPE_IDENTITY();
GO

INSERT INTO my.Objects
(
    IdObjectType
    , ObjectFlag
)
SELECT
    1 AS IdObjectType
    , 0 AS ObjectFlag

SET @IdObject = SCOPE_IDENTITY();
GO

INSERT dbo.foo
SELECT TOP (1) id FROM dbo.bar;

SET @id = SCOPE_IDENTITY();
GO

INSERT dbo.foo VALUES (1);

SET @id = SCOPE_IDENTITY();

INSERT dbo.foo VALUES (1), (2), (3);
GO
