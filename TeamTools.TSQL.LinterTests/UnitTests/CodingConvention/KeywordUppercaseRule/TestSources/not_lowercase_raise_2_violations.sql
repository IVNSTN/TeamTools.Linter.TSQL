SELECT 1
FROM dbo.foo
WHERE title not LIKE 'adsf%'    -- 1
GO

CREATE TABLE dbo.bar
(
    id INT not NULL             -- 2
)
