SELECT DISTINCT TOP (10)
    a
    , bar.b
    , bar.c
    , (b / c) as d
    , e
FROM bar

IF EXISTS( SELECT 1
    FROM t
    WHERE x = y
)
    RETURN 0;
