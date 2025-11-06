-- no aggregation
SELECT 1 AS rn, t.id, t.title AS main_title
FROM dbo.foo AS t
GO

-- fine aggregation
SELECT 1 AS rn, t.parent_id, MAX(t.date)
FROM dbo.foo AS t
GROUP BY t.parent_id
GO

-- fine aggregation
SELECT 1 AS rn, t.parent_id, MAX(t.date) OVER (PARTITION BY t.parent_id)
FROM dbo.foo AS t
GO
