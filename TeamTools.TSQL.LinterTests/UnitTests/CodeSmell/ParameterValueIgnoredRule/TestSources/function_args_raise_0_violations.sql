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

    SET @x = COALESCE(NULLIF(@a, @b), @c, CASE WHEN @d > @e THEN @f END);

    SELECT @a = NULL, @b = NULL, @c = NULL, @d = NULL, @e = NULL, @f = NULL;
END;
GO
