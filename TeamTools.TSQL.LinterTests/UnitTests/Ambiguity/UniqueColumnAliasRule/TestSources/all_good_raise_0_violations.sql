select
    t.a,
    @c,
    b,
    1 + 1 as c,
    t.c as d
FROM tbl as t
WHERE EXISTS(select a FROM e) -- ignored
