select currency_code
from dbo.currencies
union
select (SELECT 'USD') -- bad
