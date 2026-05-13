-- bare select
SELECT 1

-- no options
with cte AS(select id from foo)
select * from cte

-- no cte
select * from cte
option(FORCE ORDER)

-- fine recursive cte
with cte AS(
    select id, parent_id
    from foo
    where parent_id is  null

    union all

    select f.id, f.parent_id
    from cte c
    inner join foo f
    on f.parent_id = c.id
)
select * from cte
option(MAXRECURSION 10)
