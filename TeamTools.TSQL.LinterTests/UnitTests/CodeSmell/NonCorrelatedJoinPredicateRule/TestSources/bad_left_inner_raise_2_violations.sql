select *
from foo
    left join bar
        inner join jar
            on jar.id = @x      -- 1
        on bar.foo_id > 0       -- 2
