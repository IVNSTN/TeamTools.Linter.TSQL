SELECT COUNT(*) from foo

SELECT COUNT(0) from foo

SELECT COUNT(2) OVER(PARTITION BY group_id ORDER BY foo_date) from foo

SELECT COUNT((SELECT(SELECT(0)))) from foo
