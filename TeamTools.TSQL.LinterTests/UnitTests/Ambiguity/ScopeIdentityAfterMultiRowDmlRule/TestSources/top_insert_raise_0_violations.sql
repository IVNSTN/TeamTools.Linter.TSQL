INSERT TOP (1) dbo.foo (id)
SELECT id FROM dbo.bar;

SET @id = SCOPE_IDENTITY();
