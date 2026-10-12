INSERT dbo.foo
SELECT id FROM dbo.bar;

SET @id = SCOPE_IDENTITY();
