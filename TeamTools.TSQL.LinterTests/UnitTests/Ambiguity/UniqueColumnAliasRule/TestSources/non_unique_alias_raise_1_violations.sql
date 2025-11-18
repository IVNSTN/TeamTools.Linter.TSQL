select
    t.a,
    @c,
    b,
    1 + 1 as c,
    t.c,
    ROWGUIDCOL -- magic function, not a column name
FROM tbl as t
