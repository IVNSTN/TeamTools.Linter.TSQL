DECLARE cr CURSOR LOCAL SCROLL FOR -- non forward-only
    SELECT id
    FROM foo

OPEN cr

FETCH NEXT FROM cr
-- below are non-forward fetches
FETCH PRIOR FROM cr
FETCH FIRST FROM cr
FETCH LAST FROM cr

CLOSE cr
DEALLOCATE cr
