SELECT
    foo.title,
    CAST(NULL AS DATETIME2(3)) as start_time
FROM foo

UPDATE t SET
    lastmod = GETDATE()
    OUTPUT
        INSERTED.lastmod,
        CONVERT(BIT, NULL) as flag
FROM tmp AS t
