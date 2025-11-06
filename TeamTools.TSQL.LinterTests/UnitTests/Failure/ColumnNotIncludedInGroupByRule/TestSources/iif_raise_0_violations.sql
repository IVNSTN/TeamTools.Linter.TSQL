-- compatibility level min: 110
SELECT
    f.client_id
    , IIF(SUM(f.volume) > 0, ISNULL(f.a, f.b), f.c) AS debet
FROM dbo.foo AS f
GROUP BY
    f.client_id
    , ISNULL(f.a, f.b)
    , f.c
