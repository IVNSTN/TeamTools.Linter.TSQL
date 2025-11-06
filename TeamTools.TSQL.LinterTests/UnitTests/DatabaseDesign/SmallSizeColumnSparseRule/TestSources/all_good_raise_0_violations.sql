-- no sparse
CREATE TABLE dbo.bar
(
    bar BIT NULL,
    far TINYINT NOT NULL,
    chr CHAR(1) NULL
)
GO

-- no small
CREATE TABLE dbo.bar
(
    bar INT SPARSE NULL,
    far BIGINT SPARSE NULL,
    chr VARCHAR(1000) SPARSE NULL
)
GO
