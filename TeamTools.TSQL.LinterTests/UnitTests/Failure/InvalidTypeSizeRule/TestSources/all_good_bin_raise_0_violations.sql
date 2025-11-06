DECLARE
    @a     BINARY(1)
    , @aa  BINARY(100)
    , @aaa BINARY(8000)
    , @b   VARBINARY(1)
    , @bb  VARBINARY(100)
    , @bbb VARBINARY(8000)

CREATE TABLE #tbl
(
    a     BINARY(1)
    , aa  BINARY(100)
    , aaa BINARY(8000)
    , b   VARBINARY(1)
    , bb  VARBINARY(100)
    , bbb VARBINARY(8000)
);

SELECT
    CAST(@a AS BINARY(1))
    , CAST(@aa AS BINARY(100))
    , CAST(@aaa AS BINARY(8000))
    , CAST(@b AS VARBINARY(1))
    , CAST(@bb AS VARBINARY(100))
    , CAST(@bbb AS VARBINARY(8000))
FROM #tbl;
