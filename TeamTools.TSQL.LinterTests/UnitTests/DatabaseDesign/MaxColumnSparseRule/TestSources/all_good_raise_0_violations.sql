-- no sparse
CREATE TABLE dbo.bar
(
    bar VARBINARY(MAX),
    far VARCHAR(MAX)
)
GO

-- no max
CREATE TABLE dbo.bar
(
    bar VARBINARY(10) SPARSE NULL,
    far NCHAR(500) SPARSE NULL
)
GO
