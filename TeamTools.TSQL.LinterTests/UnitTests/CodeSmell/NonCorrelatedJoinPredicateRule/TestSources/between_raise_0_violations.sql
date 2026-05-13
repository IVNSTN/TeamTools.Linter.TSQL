SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON scm.bar.dt BETWEEN f.period_start AND dbo.foo.period_end

SELECT 1
FROM dbo.foo AS f
LEFT JOIN scm.bar AS b
    ON b.number BETWEEN 1 AND f.repeat_count
