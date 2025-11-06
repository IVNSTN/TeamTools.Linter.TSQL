SELECT 1
FROM dbo.foo
WHERE rr.p_code = CASE WHEN @curr_code = 'RUB' THEN 'RUR' ELSE @curr_code END
    AND CONVERT(VARCHAR(20), @treaty) + '-000' <> acc_code;
