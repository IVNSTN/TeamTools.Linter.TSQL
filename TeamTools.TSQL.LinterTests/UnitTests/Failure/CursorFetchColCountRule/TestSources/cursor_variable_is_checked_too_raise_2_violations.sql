DECLARE @cr CURSOR, @cursor VARCHAR(10);

SET @cursor = 'CURSOR';

set @cr = CURSOR FAST_FORWARD FOR
SELECT a, b, c FROM foo;

OPEN @cr

DECLARE @a int, @b bit, @c date, @d xml;

FETCH NEXT FROM @cr
into @a, @b, @c, @d;

FETCH NEXT FROM @cr
into @a, @b;

CLOSE @cr
DEALLOCATE cr
