-- compatibility level min: 130
-- becaust of OPENJSON
CREATE PROC dbo.foo
    @on_date DATETIME
    , @list  VARCHAR(100)
AS
BEGIN
    CREATE TABLE #t
    (
        id INT
    )

    -- not a "real DML"
    INSERT @t(id)
    SELECT 1

    SELECT @on_date = ISNULL(@on_date, EOMONTH(GETDATE()));

    SELECT @s = value
    FROM STRING_SPLIT(@list, ';')

    SELECT * FROM OPENJSON('{}')

    ALTER TABLE #t ADD title VARCHAR(100)

    INSERT #t(title)
    SELECT id
    FROM dbo.bar
    WHERE is_for_select = 1
END
GO
