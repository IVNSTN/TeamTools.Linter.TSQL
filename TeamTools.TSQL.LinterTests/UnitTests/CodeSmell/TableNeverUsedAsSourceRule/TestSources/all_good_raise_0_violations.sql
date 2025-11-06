DECLARE @foo TABLE (id INT)
CREATE TABLE #test (name varchar(100))

SELECT * FROM dbo.orders as o
inner join #test t
on t.id = o.id
where exists(select 1 from @foo)
