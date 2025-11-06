SELECT
    MAX(tbl.col)
    , AVG(tbl.col)
    , DATEADD(DD, 1, @dt)
    , TRIM('  afds ')
    , SUM(1)
    , CONCAT(NULLIF(@doc_ser + ' ', ''), t.doc_no)

PRINT FORMATMESSAGE('%s', @s)
