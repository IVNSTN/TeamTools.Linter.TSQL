SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON scm.bar.dt BETWEEN f.period_start AND dbo.foo.period_end
