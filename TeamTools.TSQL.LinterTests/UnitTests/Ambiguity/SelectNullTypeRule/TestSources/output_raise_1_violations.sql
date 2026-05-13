UPDATE t SET
    lastmod = GETDATE()
    OUTPUT
        INSERTED.lastmod,
        NULL as flag    -- here
FROM tmp AS t
