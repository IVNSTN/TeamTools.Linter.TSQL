-- no hints
SELECT *
FROM dbo.foo

DELETE dbo.foo

-- fine hints
SELECT *
FROM dbo.foo WITH (ROWLOCK, HOLDLOCK)

DELETE dbo.foo
OPTION (FORCE ORDER, MAXDOP 2)
