CREATE PROC dbo.foo
AS
BEGIN
    ALTER INDEX ix_asdf ON dbo.bar
    REBUILD
END
GO

CREATE TRIGGER dbo.foo_del ON dbo.foo AFTER DELETE
AS
BEGIN
    ALTER INDEX ix_asdf ON dbo.bar
    REBUILD
END
