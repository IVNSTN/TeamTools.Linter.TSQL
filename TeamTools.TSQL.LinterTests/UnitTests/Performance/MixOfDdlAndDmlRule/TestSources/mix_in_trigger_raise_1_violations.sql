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
    WHERE is_for_select = 1

    ALTER TABLE #t ADD title VARCHAR(100)   -- here

    INSERT #t(title)
    SELECT id
    FROM dbo.bar
    WHERE is_for_select = 1
END
GO
