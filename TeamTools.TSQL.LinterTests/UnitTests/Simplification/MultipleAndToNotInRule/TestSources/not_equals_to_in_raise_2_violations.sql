-- ... => category_id NOT IN (1, 2, 3, 4, 5)
SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)
    AND category_id <> 4
    AND category_id <> 5

-- ... => @category_id NOT IN (100, 200, 300)
IF (@category_id <> 100
    AND 200 <> @category_id
    AND @category_id <> (300))
BEGIN
    SET @bar = 'far'
END
