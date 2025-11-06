DECLARE
    @a     DECIMAL(1, 1)
    , @aa  DECIMAL(20, 0)
    , @aaa DECIMAL(38, 38)
    , @b   NUMERIC(1)
    , @bb  NUMERIC(10)
    , @bbb NUMERIC(38);

CREATE TABLE #tbl
(
    a     DECIMAL(1, 1)
    , aa  DECIMAL(20, 0)
    , aaa DECIMAL(38, 38)
    , b   NUMERIC(1)
    , bb  NUMERIC(10)
    , bbb NUMERIC(38)
);

SELECT
    CAST(@a AS DECIMAL(1, 1))
    , CAST(@aa AS DECIMAL(20, 0))
    , CAST(@aaa AS DECIMAL(38, 38))
    , CAST(@b AS NUMERIC(1, 1))
    , CAST(@bb AS NUMERIC(10))
    , CAST(@bbb AS NUMERIC(38))
FROM #tbl;
