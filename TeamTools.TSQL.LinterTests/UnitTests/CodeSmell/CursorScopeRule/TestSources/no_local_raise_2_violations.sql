DECLARE cr CURSOR GLOBAL FOR
    SELECT 1;
declare @cr CURSOR

set @cr = CURSOR FAST_FORWARD FOR
    select a from b;
