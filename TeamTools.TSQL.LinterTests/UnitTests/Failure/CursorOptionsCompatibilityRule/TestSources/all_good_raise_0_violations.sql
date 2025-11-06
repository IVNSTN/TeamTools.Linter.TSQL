DECLARE cr CURSOR FAST_FORWARD LOCAL FOR
    select 1 as id

DECLARE cr CURSOR FOR
    select 1 as id
    FOR UPDATE OF id
