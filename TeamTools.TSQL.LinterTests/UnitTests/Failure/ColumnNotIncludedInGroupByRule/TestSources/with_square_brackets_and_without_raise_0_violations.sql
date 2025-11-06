SELECT
    1
    , @var
    , t.id
    , t.[period_id] + 1
FROM dbo.foo
GROUP BY
    t.[id]
    , t.period_id
