CREATE TRIGGER aaa.trg
ON bbb.tbl
AFTER INSERT
AS
    RETURN;
GO

CREATE TRIGGER trg -- dbo omitted
ON bbb.tbl
AFTER INSERT
AS
    RETURN;
GO

CREATE TRIGGER aaa.trg
ON tbl -- dbo omitted
AFTER INSERT
AS
    RETURN;
GO
