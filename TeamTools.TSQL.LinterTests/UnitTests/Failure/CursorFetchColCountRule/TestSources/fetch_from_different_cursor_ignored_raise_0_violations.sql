DECLARE @cr CURSOR;

set @cr = CURSOR FAST_FORWARD FOR
SELECT a, b, c FROM foo;

OPEN @cr

DECLARE @a int, @b bit, @c date, @d xml;

FETCH NEXT FROM another_cursor
into @a, @b, @c, @d;
