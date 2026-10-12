DECLARE @id DECIMAL(38,0);

INSERT dbo.foo
EXEC dbo.usp_GetData;

INSERT dbo.foo
SELECT 1

SET @id = SCOPE_IDENTITY();
