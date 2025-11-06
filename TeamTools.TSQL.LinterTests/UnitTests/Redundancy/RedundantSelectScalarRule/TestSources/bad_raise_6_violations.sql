set @var = (select @another_var)
set @var = (select 1) + (select NULL)

select
    (SELECT NULL) as id
from dbo.t
where (select ((1+1))) > (select 0)
