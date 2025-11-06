DECLARE
    @cr CURSOR

SET @cr = CURSOR LOCAL FOR -- non forward-only
    SELECT id
    FROM foo

OPEN @cr

FETCH NEXT FROM @cr -- forward-only

CLOSE @cr
DEALLOCATE @cr
