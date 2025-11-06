SELECT
    1
    , @var
    , COUNT(*)
    , t.id
    , t.lastmod
    , CAST(ISNULL(t.title, '') + 'test' AS VARCHAR(MAX)) AS new_title
FROM dbo.foo
GROUP BY
    CAST(ISNULL(t.title, '') + 'test' AS VARCHAR(MAX))
    , t.id
    , t.lastmod;
