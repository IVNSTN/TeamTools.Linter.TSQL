insert @t
values (1, 2)

-- too big difference in inserted cols
insert @t
values (1, 2, 3, 4, 5, 6, 7)

insert foo
values ('')

-- insert with select source
insert foo
select 1
from bar

-- not an insert
update t set
    lastmod = getdate()
    output inserted.lastmod
    into foo (lastmod)
from bar
