SELECT *
FROM dbo.fo
GROUP BY
    GROUPING SETS (
        (a)
        , (a, b)
    );

SELECT *
FROM dbo.fo
GROUP BY
    GROUPING SETS
    (
        (a)
        , (a, b)
    );

SELECT *
FROM dbo.fo
GROUP BY GROUPING SETS((), (a));

MERGE t WITH (HOLDLOCK) AS trg
USING (SELECT * FROM @src) AS src
ON trg.id = src.id
WHEN NOT MATCHED THEN
    INSERT (title)
    VALUES (src.title)
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN MATCHED THEN
    UPDATE SET
        title = src.title,
        lastmod = SYSDATETIME()
OUTPUT DELETED.title AS old_title, $action AS act
INTO @log(old_title, act);
GO
