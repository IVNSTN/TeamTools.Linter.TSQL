select
    master.dbo.sysobjects.id,
    dbo.balance.name
from balance
inner join master.dbo.sysobjects
    on id = obj_id
