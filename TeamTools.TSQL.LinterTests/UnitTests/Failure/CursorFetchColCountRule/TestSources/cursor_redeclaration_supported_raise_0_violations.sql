DECLARE @a int, @b bit, @c date;

DECLARE my_cur CURSOR FAST_FORWARD FOR
SELECT a, b, c FROM foo;

OPEN my_cur

FETCH NEXT FROM my_cur
into @a, @b, @c;

CLOSE my_cur
DEALLOCATE my_cur

-- name reuse

DECLARE my_cur CURSOR FAST_FORWARD FOR
SELECT a FROM foo;

OPEN my_cur

FETCH NEXT FROM my_cur
into @a;

CLOSE my_cur
DEALLOCATE my_cur
