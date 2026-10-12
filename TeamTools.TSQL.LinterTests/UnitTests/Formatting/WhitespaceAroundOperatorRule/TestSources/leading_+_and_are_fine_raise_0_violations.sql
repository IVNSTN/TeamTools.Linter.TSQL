SELECT
    CASE WHEN op_code = 'BUY'
        THEN +1
        ELSE -1
    END,
    CASE WHEN op_code = 'BUY'
        THEN
        +volume
        ELSE
        -volume
    END,
- (a + b) as sum_minus,
+ (a - b) as sum_plus

SELECT
    -price * amount
    , +volume / -gradient
WHERE
    discount BETWEEN -100 AND 100
    OR title LIKE 'asdf'
