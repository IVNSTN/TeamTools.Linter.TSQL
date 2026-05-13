-- fine looking filter
select *
from orders
where client_id = @client_id OR @client_id IS NULL

-- no col involved
select *
from orders
where ISNULL(@var, 0) = 123

-- different cols
select *
from orders
where client_id = ISNULL(@client_id, company_id)

-- not a column on the left side
select *
from orders o
join clients c
on o.client_id = c.client_id
AND @filter_value = ISNULL(@tariff_id, c.tariff_id)

-- not a var in ISNULL
select *
from orders
where client_id = ISNULL(dbo.my_func(), client_id)

-- a > b
select *
from clients c
WHERE c.tariff_id > COALESCE(@tariff_id, c.tariff_id)
