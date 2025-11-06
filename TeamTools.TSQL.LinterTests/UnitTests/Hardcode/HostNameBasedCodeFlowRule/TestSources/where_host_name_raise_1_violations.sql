UPDATE t SET
    foo = bar
FROM tbl as t
WHERE 'asdf' <> HOST_NAME()
