if not exists(select 0 where 0 = 1)
select a
from t
where exists(select 1)
or exists(select 0/0 from tt where tt.name = 'asfd')
and not exists(select null from yy)
