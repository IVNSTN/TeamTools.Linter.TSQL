DECLARE cr CURSOR LOCAL SCROLL FOR -- non forward-only
    SELECT id
    FROM foo

OPEN cr

FETCH NEXT FROM cr -- forward-only

CLOSE cr
DEALLOCATE cr
GO
DECLARE cr CURSOR LOCAL FOR -- non forward-only
    SELECT id
    FROM foo

OPEN cr

FETCH NEXT FROM cr -- forward-only

CLOSE cr
DEALLOCATE cr
