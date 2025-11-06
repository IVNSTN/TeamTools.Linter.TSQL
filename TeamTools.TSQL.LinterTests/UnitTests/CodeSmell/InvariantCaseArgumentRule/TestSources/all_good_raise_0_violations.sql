SET @var = CASE
    WHEN GETDATE() > @today THEN 2
    ELSE 3
END

SELECT COALESCE(id, 3)
FROM dbo.foo
