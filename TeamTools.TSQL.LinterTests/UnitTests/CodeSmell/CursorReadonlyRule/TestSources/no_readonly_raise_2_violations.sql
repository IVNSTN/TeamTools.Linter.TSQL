DECLARE cr CURSOR FOR
    SELECT 1;
declare @cr CURSOR

set @cr = CURSOR LOCAL FOR
    select a from b;
