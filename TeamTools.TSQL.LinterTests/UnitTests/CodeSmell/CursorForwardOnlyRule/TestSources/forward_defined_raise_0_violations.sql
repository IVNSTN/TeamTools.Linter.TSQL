DECLARE cr CURSOR LOCAL FORWARD_ONLY FOR -- forward-only
    SELECT id
    FROM foo

OPEN cr

FETCH NEXT FROM cr -- forward-only

CLOSE cr
DEALLOCATE cr
GO
DECLARE cr CURSOR LOCAL FAST_FORWARD FOR -- fast-forward
    SELECT id
    FROM foo

OPEN cr

FETCH FROM cr -- forward-only - NEXT is default direction

CLOSE cr
DEALLOCATE cr
