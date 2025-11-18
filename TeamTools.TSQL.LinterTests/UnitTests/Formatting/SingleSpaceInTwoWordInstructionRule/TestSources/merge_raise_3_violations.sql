MERGE t WITH (HOLDLOCK) AS trg
USING (SELECT * FROM @src) AS src
ON trg.id = src.id
WHEN    NOT MATCHED AND (1=1) THEN      -- 1
    INSERT (title)
    VALUES (src.title)
WHEN NOT    MATCHED BY SOURCE THEN      -- 2
    DELETE
WHEN MATCHED    THEN                    -- 3
    UPDATE SET
        title = src.title,
        lastmod = SYSDATETIME()
OUTPUT DELETED.title AS old_title, $action AS act
INTO @log(old_title, act);
