-- compatibility level min: 110
select v.*
from
(
select a, b, c
for xml auto
) as v(pkg)
