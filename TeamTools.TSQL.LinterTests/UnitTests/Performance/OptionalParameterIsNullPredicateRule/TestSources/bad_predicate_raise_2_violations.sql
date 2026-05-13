select *
from orders o
where ((ISNULL((@client_id), o.client_id)) = (o.client_id))

select *
from orders o
join clients c
on o.client_id = c.client_id
AND c.tariff_id = COALESCE(@tariff_id, c.tariff_id)
