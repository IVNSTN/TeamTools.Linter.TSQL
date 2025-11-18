SELECT o.IdObjectType, SUM(IdObjectType)
FROM dbo.Objects AS o
GROUP BY IdObjectType
HAVING COUNT(IdObjectType) > 1000

SELECT o.IdObjectType, COUNT(1)
FROM dbo.Objects AS o
GROUP BY IdObjectType
HAVING IdObjectType > COUNT(1)
