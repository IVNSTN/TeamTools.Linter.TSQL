TRUNCATE     TABLE dbo.foo

INSERT    INTO dbo.foo (id)
VALUES (1)

DELETE   FROM dbo.foo
WHERE 1 = 0

DELETE   FROM t
FROM dbo.tbl AS t
WHERE 1 = 0
