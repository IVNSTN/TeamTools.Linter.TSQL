-- compatibility level min: 110
SELECT
    MAX(t.distance) OVER (ORDER BY t.zone ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS mdist_rows
    , MIN(t.distance) OVER (ORDER BY t.zone RANGE UNBOUNDED PRECEDING) AS mdist_range;
