DECLARE cr CURSOR READ_ONLY FOR
    SELECT 1;
declare @cr CURSOR

set @cr = CURSOR LOCAL FAST_FORWARD FOR -- same as readonly
    select a from b;

set @cr = CURSOR LOCAL FOR
    select a from b
    FOR UPDATE OF a;
