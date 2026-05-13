CREATE PROC dbo.foo
AS
BEGIN
    CREATE TABLE #t
    (
        id INT
    )

    INSERT #t(id)
    SELECT id
    FROM dbo.bar
    WHERE is_for_select = 1

    ALTER TABLE #t ADD title VARCHAR(100)

    INSERT #t(title)
    SELECT id
    FROM dbo.bar
    WHERE is_for_select = 1
    OPTION (RECOMPILE)          -- recompilaton is what we want
END
GO
