CREATE TRIGGER dbo.foo ON dbo.bar
AFTER INSERT
AS
BEGIN
    DECLARE @foo TABLE (id INT)
    INSERT @foo values(1);
END
GO
