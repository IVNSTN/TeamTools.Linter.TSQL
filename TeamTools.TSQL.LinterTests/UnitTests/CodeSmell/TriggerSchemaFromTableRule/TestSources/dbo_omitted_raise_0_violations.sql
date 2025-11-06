-- missing schema in object creation is controlled by a separate rule
CREATE TRIGGER trg -- dbo omitted
ON dbo.tbl
AFTER INSERT
AS
    RETURN;
GO

CREATE TRIGGER dbo.trg
ON tbl -- dbo omitted
AFTER INSERT
AS
    RETURN;
GO
