SELECT f.id
    , DATEPART(HOUR, f.first_time)
    , @@SERVERNAME AS srv_name
    , DATEADD(DD, 1, f.last_date)
    , CAST(0 AS INT) AS zero
FROM dbo.foo f
WHERE EXISTS(SELECT 1 FROM dbo.bar)
ORDER BY (SELECT NEWID())
