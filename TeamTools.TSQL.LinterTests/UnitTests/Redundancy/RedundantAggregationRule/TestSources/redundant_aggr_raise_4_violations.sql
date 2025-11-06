SELECT
    MAX(DISTINCT 1) AS m
    , SUM((0)) AS s
    , AVG(100) AS a
    , COUNT(NULL) AS a
FROM dbo.foo AS t
GO
