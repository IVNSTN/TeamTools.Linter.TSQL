MERGE dbo.foo AS f
USING
(
    -- 1 (for all items)
    SELECT id, title
    FROM dbo.bar ba
    INNER JOIN dbo.far fa
        ON id = parent_id
) AS b
ON f.id = b.parent_id
WHEN MATCHED THEN
    UPDATE SET title = b.title
WHEN NOT MATCHED THEN
    INSERT (id, title)
    VALUES
    (b.id, b.title);
GO

MERGE dbo.foo AS f
USING
(
    SELECT fa.id, fa.title
    FROM dbo.bar ba
    INNER JOIN dbo.far fa
        ON fa.id = ba.parent_id
) AS b
ON f.id = parent_id -- 2
WHEN MATCHED THEN
    UPDATE SET title = b.title
WHEN NOT MATCHED THEN
    INSERT (id, title)
    VALUES
    (b.id, b.title);
GO

MERGE dbo.foo AS f
USING
(
    SELECT ba.id, fa.title
    FROM dbo.bar ba
    INNER JOIN dbo.far fa
        ON fa.id = ba.parent_id
) AS b
ON f.id = b.parent_id
WHEN MATCHED THEN
    UPDATE SET title = title -- 3
WHEN NOT MATCHED THEN
    INSERT (id, title)
    VALUES
    (b.id, b.title);
GO

MERGE dbo.foo AS f
USING
(
    SELECT ba.id, fa.title
    FROM dbo.bar ba
    INNER JOIN dbo.far fa
        ON fa.id = ba.parent_id
) AS b
ON f.id = b.parent_id
WHEN MATCHED THEN
    UPDATE SET title = b.title
WHEN NOT MATCHED THEN
    INSERT (id, title)
    VALUES
    (id, title); -- 4
