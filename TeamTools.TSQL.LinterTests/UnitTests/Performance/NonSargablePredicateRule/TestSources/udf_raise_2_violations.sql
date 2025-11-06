SELECT *
FROM dbo.foo f
INNER JOIN dbo.bar b
    ON f.id > dbo.min_id()
WHERE f.date_open BETWEEN dbo.yesterday() AND GETDATE()
