    SELECT
        r.long_name
        , CASE WHEN method = '3071' THEN 'LIFO' ELSE 'FIFO' END
    FROM dbo.a_tax_methods AS m
, taxes.tax_residence AS r
WHERE m.treaty = 123
and m.residence = r.residence
    AND m.year = YEAR(GETDATE());
