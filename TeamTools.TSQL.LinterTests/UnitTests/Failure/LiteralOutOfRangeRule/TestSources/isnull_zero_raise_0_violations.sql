DECLARE
    @a   BIT
    , @b BIT
    , @c TINYINT
    , @d TINYINT
    , @e INT
    , @f INT;

SELECT
    @a = ISNULL(@a, 0)
    , @b = ISNULL(@a, 0)
    , @c = ISNULL(@a, 0)
    , @d = ISNULL(@a, 0)
    , @e = ISNULL(@a, 0)
    , @f = ISNULL(@a, 0);
