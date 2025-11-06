-- compatibility level min: 110
CREATE PROCEDURE dbo.bar
    @a INT
    , @b INT
    , @c INT
    , @d INT
    , @e INT
    , @f INT
AS
BEGIN
    DECLARE @x INT;

    SET @x = TRY_CONVERT(TINYINT, @a)
        + TRY_CAST(@b AS NUMERIC(10, 2))
        + IIF (@c > @d, @e, @f);

    SELECT @a = NULL, @b = NULL, @c = NULL, @d = NULL, @e = NULL, @f = NULL;
END;
GO
