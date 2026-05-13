select client_id, account_id, sum(val) val
from my_table
group by client_id, cube(account_id)

select client_id, isnull(account_id, 0) as account_id, sum(val) val
from my_table
group by client_id, cube(account_id)
