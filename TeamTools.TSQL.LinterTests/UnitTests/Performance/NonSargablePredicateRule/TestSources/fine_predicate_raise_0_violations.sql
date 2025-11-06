SELECT *
FROM dbo.foo f
WHERE f.id > @min_id
AND t.title IS NOT NULL
AND t.last_date < GETDATE()
