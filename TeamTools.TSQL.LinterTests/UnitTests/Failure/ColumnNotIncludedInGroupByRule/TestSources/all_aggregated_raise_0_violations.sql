SELECT
    1
    , @var
    , t.id
    , MAX(t.lastmod)
    , CASE t.id
        WHEN 1 THEN MAX(t.lastmod)
        ELSE 0
      END AS flag_a
    , CASE
        WHEN (t.id > 1) THEN (SELECT (t.id))
        ELSE MAX(t.lastmod)
      END AS flag_b
FROM dbo.foo
GROUP BY t.id;
