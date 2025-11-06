set @var = (select t.name from my_tbl)

select
    t.*,
    row_number() over(order by (select null)) rn -- must be ignored
from tbl
order by (select 1) -- must be ignored
