SELECT
    trd_no
    , treaty
FROM #trades AS tr
WHERE EXISTS (SELECT 1 FROM #trades AS trin WHERE trin.ts_no = tr.ts_no);
