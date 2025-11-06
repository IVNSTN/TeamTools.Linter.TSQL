SELECT
    CASE WHEN op_code = 'BUY'
    THEN +1
    ELSE -1
END,
- (a + b) as sum_minus, -- space required before (
+ (a - b) as sum_plus   -- space required before (
