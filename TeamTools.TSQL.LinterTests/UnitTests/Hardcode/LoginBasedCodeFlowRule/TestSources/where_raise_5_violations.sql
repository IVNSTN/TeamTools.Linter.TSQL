UPDATE t SET
    foo = bar
FROM tbl as t
WHERE 'asdf' <> ORIGINAL_LOGIN()
    AND t.name <> SUSER_SNAME()
    AND t.name <> SYSTEM_USER
    AND t.name <> CURRENT_USER
    AND t.name <> SESSION_USER
