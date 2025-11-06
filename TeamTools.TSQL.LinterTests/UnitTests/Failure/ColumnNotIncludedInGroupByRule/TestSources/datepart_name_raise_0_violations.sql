SELECT
    1
    , DATEPART(YEAR, t.open_date) AS open_year
FROM dbo.foo
GROUP BY t.open_date;
