DECLARE
    @a     VARCHAR(1)
    , @aa  VARCHAR(100)
    , @aaa VARCHAR(8000)
    , @b   NVARCHAR(1)
    , @bb  NVARCHAR(100)
    , @bbb NVARCHAR(4000)
    , @c   CHAR(1)
    , @c   CHAR(100)
    , @c   CHAR(8000)
    , @d   NCHAR(1)
    , @dd  NCHAR(100)
    , @dd  NCHAR(4000);

CREATE TABLE #tbl
(
    a     VARCHAR(1)
    , aa  VARCHAR(100)
    , aaa VARCHAR(8000)
    , b   NVARCHAR(1)
    , bb  NVARCHAR(100)
    , bbb NVARCHAR(4000)
    , c   CHAR(1)
    , c   CHAR(100)
    , c   CHAR(8000)
    , d   NCHAR(1)
    , dd  NCHAR(100)
    , dd  NCHAR(4000)
);

SELECT
    CAST(@a AS VARCHAR(1))
    , CAST(@aa AS VARCHAR(100))
    , CAST(@aaa AS VARCHAR(8000))
    , CAST(@b AS NVARCHAR(1))
    , CAST(@bb AS NVARCHAR(100))
    , CAST(@bbb AS NVARCHAR(4000))
    , CAST(@c AS CHAR(1))
    , CAST(@c AS CHAR(100))
    , CAST(@c AS CHAR(8000))
    , CAST(@d AS NCHAR(1))
    , CAST(@dd AS NCHAR(100))
    , CAST(@dd AS NCHAR(4000))
FROM #tbl;
