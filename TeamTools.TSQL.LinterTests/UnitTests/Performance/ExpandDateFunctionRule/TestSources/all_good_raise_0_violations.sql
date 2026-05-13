-- no func in predicate
SELECT sum(payment)
FROM taxes
WHERE pay_period >= @tax_year_begin
    AND pay_period < @next_tax_year

-- YEAR can be precomputed
SELECT sum(payment)
FROM taxes
JOIN report_periods
ON tax_period = @report_period_start
AND YEAR(@report_period_start) = @tax_year

-- can't be fixed
SELECT sum(payment)
FROM taxes AS t
WHERE YEAR(t.pay_period) = t.some_other_year
