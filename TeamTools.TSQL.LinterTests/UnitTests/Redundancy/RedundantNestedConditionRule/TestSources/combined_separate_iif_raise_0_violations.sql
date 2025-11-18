-- compatibility level min: 110
SELECT 1
FROM dbo.bar AS b
WHERE
    b.far = IIF(b.is_non_restricted = 'Y', 1, 0)
    AND b.jar = IIF(b.is_non_restricted = 'Y', 200, 300)    -- IIF predicate is not a dup of the prior IIF
