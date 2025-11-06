DECLARE
    @aa  BINARY(0)
    , @aaa BINARY(9000)
    , @bb  VARBINARY(0)
    , @bbb VARBINARY(9000)

CREATE TABLE #tbl
(
    aa  BINARY(0)
    , aaa BINARY(9000)
    , bb  VARBINARY(0)
    , bbb VARBINARY(9000)
);

SELECT
     CAST(@aa AS BINARY(0))
    , CAST(@aaa AS BINARY(10000))
    , CAST(@bb AS VARBINARY(0))
    , CAST(@bbb AS VARBINARY(9000))
FROM #tbl;
