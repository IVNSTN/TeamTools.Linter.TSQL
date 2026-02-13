-- bare select
SELECT 1

-- no FROM
SELECT 1
WHERE 1=1

-- no JOIN
SELECT *
FROM foo
WHERE foo.group_id = 123

SELECT 1
FROM dbo.foo AS f
INNER JOIN @bar AS b
    ON b.group_id = f.group_id
INNER JOIN dbo.car AS c
    ON c.id = b.id

SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON scm.bar.id > f.id
LEFT JOIN asdf.far
    ON bar.title != ''
    AND asdf.far.something = b.something

-- both OR parts are fine
SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON scm.bar.id > f.id
    OR foo.group_id = ISNULL(b.group_id, -1)

-- left inner
select *
from foo
    left join bar
        inner join jar
            on bar.another_id = jar.id
        on bar.foo_id = foo.id

-- bunch of joins to dictionaries
select 1
from trades as t
inner join products as p
    on p.id = t.product_id
inner join locations as l
    on l.id = t.location_id
left join defects as d
    on d.trade_id = t.id
outer apply
(
    select top 1 c.descr
    from complaints c
    where c.trade_id = t.id
    order by c.dt desc
) as c
left join
(
    select top 10 tt.id
    from trades tt
    where tt.product_id = t.product_id
    order by tt.dt desc
) tt
on tt.merchant_id = t.merchant_id

