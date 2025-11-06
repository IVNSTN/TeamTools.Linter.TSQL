SELECT
    COALESCE(t.id, @var, '1', '1') -- '1'
    , COALESCE(t.id, @var, @c, @e, @c) -- @c
    , COALESCE(t.id, DATEADD(DD, 1, GETDATE() + 2), @d, DATEADD(DD, 1, GETDATE() + 2)) -- DATEADD
