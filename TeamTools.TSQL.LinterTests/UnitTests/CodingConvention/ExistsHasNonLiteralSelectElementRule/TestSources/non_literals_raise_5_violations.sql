if not exists(select zzz.xxx where 0 = 1)
select a
from t
where exists(select a.id)
or exists(select tt.name as nm from tt where tt.name = 'asfd')
and not exists(select b from yy)
or exists(select (select top 1 id from b) from c)
