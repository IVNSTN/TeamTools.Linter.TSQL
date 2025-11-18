DECLARE
    @d DECIMAL(18, 1)
    , @r NUMERIC(0, 5)      -- scale is fine; there is a separate rule for illegal precision


SELECT CONVERT(DECIMAL(5, 2), 1/2) AS result
