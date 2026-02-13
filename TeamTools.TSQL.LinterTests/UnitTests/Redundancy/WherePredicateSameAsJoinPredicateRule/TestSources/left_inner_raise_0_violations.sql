select *
from foo
left join bar
    inner join far
        on far.group_id = bar.group_id
            on bar.id = foo.id
            and @var > 100
where @var > 100
