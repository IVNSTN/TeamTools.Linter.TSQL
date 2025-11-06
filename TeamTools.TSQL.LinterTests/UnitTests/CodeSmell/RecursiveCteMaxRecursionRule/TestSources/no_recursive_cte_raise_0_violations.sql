SELECT id
FROM dbo.foo
GO

WITH cte AS
(
    SELECT id
    FROM dbo.bar
)
SELECT f.*
FROM dbo.foo f
INNER JOIN cte e
ON e.id = f.id
