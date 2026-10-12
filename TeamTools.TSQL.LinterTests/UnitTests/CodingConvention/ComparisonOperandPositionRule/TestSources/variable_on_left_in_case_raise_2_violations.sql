-- Wrong: literal on left side
SELECT 1 FROM dbo.foo WHERE @title <> foo.title

IF @min_value < dbo.my_fn() RETURN
