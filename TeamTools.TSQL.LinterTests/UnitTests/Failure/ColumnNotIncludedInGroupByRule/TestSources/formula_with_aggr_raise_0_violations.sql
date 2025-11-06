SELECT
    SUM(t.rate) / MAX(x.divider)
    , -1 * (AVG(t.volume))
    , (CASE
           WHEN (t.group_id > 1) THEN
               CAST(MIN(t.volume) AS BIGINT)
           ELSE
               MAX(CAST((-t.volume) AS BIGINT))
       END
      ) * 2
FROM #tbl AS t
GROUP BY t.group_id;
