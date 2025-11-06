CREATE TABLE #tbl
(
    a     VARCHAR(0) -- with negative numbers parsing fails
    , aaa VARCHAR(10000)
    , b   NVARCHAR(0)
    , bbb NVARCHAR(8000)
    , c   CHAR(0)
    , c   CHAR(9000)
    -- , c   CHAR(MAX) parsing fails, not possible to test
    , d   NCHAR(0)
    -- , dd  NCHAR(MAX) parsing fails, not possible to test
    , dd  NCHAR(8000)
);
