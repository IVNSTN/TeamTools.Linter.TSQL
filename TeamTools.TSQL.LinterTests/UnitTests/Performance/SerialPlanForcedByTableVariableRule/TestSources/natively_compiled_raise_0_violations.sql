-- compatibility level min: 130
CREATE PROC dbo.foo
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
    DECLARE @t TABLE (id INT)

    INSERT @t(id)
    SELECT id
    FROM bar
    INNER JOIN far
        on id = parent_id
    WHERE is_disabled = 0

    RETURN 1
END
GO
