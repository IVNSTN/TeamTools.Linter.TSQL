SELECT sum(payment)
FROM taxes
WHERE YEAR(pay_period) = @tax_year                          -- 1

SELECT sum(payment)
FROM taxes
WHERE (DATEPART(YEAR, ((pay_period)))) = @tax_year          -- 2
OR DATENAME(YEAR, pay_period) = '2020'                      -- 3

SELECT sum(payment)
FROM taxes
JOIN report_periods
ON (@report_period) = DATETRUNC(YEAR, tax_period)           -- 4

SELECT sum(payment)
FROM taxes
JOIN report_periods
ON DATEADD(DAY, 1, pay_date) < @max_date                    -- 5
