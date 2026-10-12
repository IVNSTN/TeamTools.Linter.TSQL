DECLARE @id DECIMAL(38,0);

INSERT dbo.foo
EXEC dbo.usp_GetData;

SET @id = SCOPE_IDENTITY();
