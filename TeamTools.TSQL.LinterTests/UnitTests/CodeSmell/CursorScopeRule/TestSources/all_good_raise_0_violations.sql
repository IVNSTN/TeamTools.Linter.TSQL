DECLARE cr CURSOR LOCAL FOR
    SELECT 1;
declare @cr CURSOR

set @cr = CURSOR LOCAL FAST_FORWARD FOR
    select a from b;
