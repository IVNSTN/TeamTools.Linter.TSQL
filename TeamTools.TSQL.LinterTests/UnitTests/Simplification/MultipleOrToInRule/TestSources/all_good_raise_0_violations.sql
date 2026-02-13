SELECT *
FROM dbo.foo
WHERE category_id IN (1, 2, 3)

SELECT *
FROM dbo.foo
WHERE category_id IN (1, 2, 3)
    OR is_root = 1

SELECT *
FROM dbo.foo
WHERE category_id = 1
    AND category_id <> @excluded_id

IF (@category_id IS NOT NULL
    AND (@category_id > 100 OR @category_id < 30))
    OR @has_no_category = 1
BEGIN
    SET @bar = 'far'
END
