SELECT
    1  + 1, -- 1
    2 *  2, -- 2
    3  /  3, -- 3
    4  %    4, -- 4
    '1+1' as [2-2]
WHERE
    6  -   6 =    0 -- 5

set @a +=   1 -- 6
select @b  -=   CASE WHEN ( -3   <= 4) THEN 0 ELSE 1 end; -- 7, 8, 9

SELECT COUNT( * ) FROM foo -- 10
