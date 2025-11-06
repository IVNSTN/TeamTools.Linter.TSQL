select a, b, c
from t1

select b, c
from t2
UNION ALL
select b, e
from t3 
inner join
(
    select e, f, g, h from t4 
    except  
    select e, f, g, null from t5
) t4 on t4.e = t3.b

select 1 as a
intersect
select a as b from c
