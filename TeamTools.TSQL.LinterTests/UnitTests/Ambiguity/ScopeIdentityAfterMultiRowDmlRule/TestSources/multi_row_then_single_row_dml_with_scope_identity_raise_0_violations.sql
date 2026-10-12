DECLARE @id DECIMAL(38,0);

INSERT dbo.foo VALUES (1), (2), (3);

INSERT dbo.bar VALUES (1);

SET @id = SCOPE_IDENTITY();
