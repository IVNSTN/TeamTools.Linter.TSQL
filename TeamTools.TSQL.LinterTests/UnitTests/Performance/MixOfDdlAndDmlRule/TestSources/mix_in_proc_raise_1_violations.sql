CREATE PROC dbo.foo
AS
BEGIN
    CREATE TABLE #t
    (
        id INT
    )

    SELECT id
    FROM dbo.bar
    INNER JOIN #t
    ON id = parent_id
    WHERE is_for_select = 1

    ALTER TABLE #t ADD title VARCHAR(100)   -- here

    DELETE bar
    FROM dbo.bar bar
    INNER JOIN #t t ON t.id = bar.id
    OPTION (FORCE ORDER)
END
GO
