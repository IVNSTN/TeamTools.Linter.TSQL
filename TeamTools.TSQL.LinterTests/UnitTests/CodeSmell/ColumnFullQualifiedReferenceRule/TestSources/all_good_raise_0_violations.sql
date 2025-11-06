select a,
    t.b as c
from emails as t

DECLARE @x XML

select
    a.data.value('@treaty', 'VARCHAR(10)') + '-000' AS acc_code
FROM @x.nodes('payments_list/payment') AS a(data);
