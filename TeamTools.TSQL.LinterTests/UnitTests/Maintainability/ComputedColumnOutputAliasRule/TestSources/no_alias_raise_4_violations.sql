select @who, -- 1
    (select a from bar), -- 2
    foo.c,
    null -- 3
from
(
    select * from zar
) zar
INNER JOIN
(
    SELECT 3 * 3 -- 4
    union all
    select 4 * 4 -- ignored
) f
ON id = id
where exists (select 1 from tbl) -- ignored
