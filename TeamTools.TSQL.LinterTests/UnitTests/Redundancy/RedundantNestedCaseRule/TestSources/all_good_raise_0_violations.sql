-- combination of searched and simple cases is ignored
SET @a =
    CASE @b
        WHEN 1 THEN 'a'
        WHEN 2 THEN 'b'
        ELSE
            CASE
                WHEN @c = 0 THEN 'c'
            END
    END;

SET @a =
    CASE
        WHEN @b = 1 THEN 'a'
        WHEN @b = 2 THEN 'b'
        ELSE
            CASE @b
                WHEN 0 THEN 'c'
            END
    END;

-- both are simple but have different predicate
SET @a =
    CASE @a
        WHEN 1 THEN 'a'
        WHEN 2 THEN 'b'
        ELSE
            CASE @b
                WHEN 0 THEN 'c'
            END
    END;

SELECT CASE WHEN 1=0 THEN 0 END
