-- empty body
CREATE PROC dbo.foo
AS;
GO

CREATE TRIGGER dbo.foo ON dbo.bar
AFTER INSERT
AS
BEGIN
    return;
end;
GO

-- fine order
CREATE PROC dbo.foo
AS
BEGIN
    CREATE TABLE #t
    (
        id INT
    )

    CREATE INDEX ix ON #t(id)

    UPDATE t SET
        lastmod = GETDATE()
    FROM dbo.bar as t
    WHERE is_for_select = 1
END
GO

CREATE TRIGGER dbo.foo ON dbo.bar
AFTER INSERT
AS
BEGIN
    CREATE TABLE #t
    (
        id INT
    )

    INSERT #t(id)
    SELECT id
    FROM INSERTED
END
GO
