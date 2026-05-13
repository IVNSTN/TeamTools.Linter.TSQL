select new.new_value, old.old_value
from src as old
inner join old as new
    on id = id
inner join new as src
    on id = id
where new.new_value <> old.old_value
and src.need_check = 1
