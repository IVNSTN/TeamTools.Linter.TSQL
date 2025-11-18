-- FIXME : must raise 4 violations
CREATE TABLE dbo.mytbl
(
    id INT,
    CONSTRAINT CK_ID CHECK (id <> SYSDATETIME())
)
GO
CREATE TABLE #mytbl
(
    id TIME,
    CONSTRAINT CK_ID CHECK (id = CAST(GETDATE() AS DATE) OR id = NEWID())
)
GO
DECLARE @mytbl TABLE
(
    id INT NULL CHECK (id < NEWID())
)
GO
