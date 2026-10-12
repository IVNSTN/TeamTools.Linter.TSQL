INSERT dbo.foo
OUTPUT INSERTED.id INTO @ids
VALUES (1), (2), (3);

SET @some_of_id = SCOPE_IDENTITY()
