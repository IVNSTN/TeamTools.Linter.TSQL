DECLARE
    @d DECIMAL              -- 1 the default scale is 0 (precision = 18)
    , @r NUMERIC(1, 0)      -- 2

SELECT CONVERT(DECIMAL(5, 0), 1/2) AS result    -- 3
