SELECT
    1
    , @var
    , (t.id) AS grouped_id
    , t.lastmod -- 1 t.lastmod
    , CAST(ISNULL(t.title, '') + 'test' AS VARCHAR(MAX)) AS new_title -- 2 t.title
FROM dbo.foo
GROUP BY t.id;
