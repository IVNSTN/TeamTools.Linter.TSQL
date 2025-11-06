DECLARE
    @a   BIT
    , @c TINYINT
    , @d INT

SET @a = ISNULL(@a, 2)              -- 1
SET @c = ISNULL(@c, -1)             -- 2
SET @d = ISNULL(@d, 1234567890123)  -- 3
