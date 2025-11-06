DECLARE cr CURSOR LOCAL FORWARD_ONLY FOR
    SELECT ((1 + tbl.c) / 2) AS a, @e b, tbl.c
    FROM tbl
    FOR UPDATE OF a, b

DECLARE cr CURSOR LOCAL FORWARD_ONLY FOR
    SELECT @e AS b, tbl.c
    FROM tbl
    FOR UPDATE OF b
