-- ... => category_id IN (1, 2, 3)
SELECT *
FROM dbo.foo
WHERE category_id = 1
    OR (2 = category_id)
    OR title <> 'asfd'
    OR (CATEGORY_ID = (3))
    OR id > 100
    OR 0 = @take_all

-- ... => @category_id IN (100, 200, 300)
IF (@category_id = 100
    OR 200 = @category_id
    OR @category_id = (300))
BEGIN
    SET @bar = 'far'
END
