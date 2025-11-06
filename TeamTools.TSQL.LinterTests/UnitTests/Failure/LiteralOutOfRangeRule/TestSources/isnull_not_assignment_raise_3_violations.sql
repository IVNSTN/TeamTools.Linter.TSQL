DECLARE
    @a   BIT
    , @c TINYINT
    , @d INT

SELECT ISNULL(@a, 2)              -- 1

PRINT ISNULL(@c, -1)              -- 2

IF 1 <> ISNULL(@d, 1234567890123) -- 3
    RETURN
