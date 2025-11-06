DECLARE @a int, @b bit, @c date;

DECLARE my_cur CURSOR FAST_FORWARD FOR
SELECT a, b, c FROM foo
union 
SELECT a, b, c FROM bar
union all
SELECT a, b, c FROM zar

OPEN my_cur

FETCH NEXT FROM my_cur
into @a, @b, @c;

CLOSE my_cur
DEALLOCATE my_cur
