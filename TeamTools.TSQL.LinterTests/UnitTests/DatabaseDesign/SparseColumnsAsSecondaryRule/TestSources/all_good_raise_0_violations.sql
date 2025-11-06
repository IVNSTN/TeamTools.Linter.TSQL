-- no sparse cols
CREATE TABLE dbo.bar
(
    bar INT NOT NULL
)
GO

-- only sparse cols
CREATE TABLE dbo.bar
(
    bar INT SPARSE NULL,
    far BIT SPARSE NULL
)
GO

-- most cols are sparse
CREATE TABLE dbo.bar
(
    id  INT  NOT NULL,
    dt  DATE NOT NULL DEFAULT GETDATE(),
    bar INT SPARSE NULL,
    far BIT SPARSE NULL,
    jar CHAR(1) SPARSE NULL
)
GO
