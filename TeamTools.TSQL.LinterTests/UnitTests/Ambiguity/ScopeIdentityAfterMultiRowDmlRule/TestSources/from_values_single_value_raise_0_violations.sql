INSERT dbo.foo
SELECT id
FROM
(
    VALUES(1, 2, 3)
) v (id)

SET @id = SCOPE_IDENTITY();
