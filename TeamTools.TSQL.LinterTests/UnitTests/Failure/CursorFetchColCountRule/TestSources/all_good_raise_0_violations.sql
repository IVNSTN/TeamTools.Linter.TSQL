SELECT 1 from [cursor]

DECLARE @cr CURSOR, @cursor INT = 'CURSOR';

DECLARE cr CURSOR FAST_FORWARD FOR
SELECT a, b, c FROM foo;

OPEN cr

DECLARE @a int, @b bit, @c date;

FETCH NEXT FROM cr
into @a, @b, @c;

FETCH NEXT FROM cr;

CLOSE cr
DEALLOCATE cr
