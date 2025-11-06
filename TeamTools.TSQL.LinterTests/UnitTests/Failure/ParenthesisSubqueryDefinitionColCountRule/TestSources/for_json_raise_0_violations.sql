-- compatibility level min: 130
SELECT v.*
from
(
select a, b, c
for json path
) as v(pkg)
