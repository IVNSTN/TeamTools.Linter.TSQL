-- no cte
select * from cte
option(MAXRECURSION 10)

-- cte is not recursive
with cte AS(
    select id, parent_id
    from foo
    where parent_id is  null
)
select * from cte
option(MAXRECURSION 10)
