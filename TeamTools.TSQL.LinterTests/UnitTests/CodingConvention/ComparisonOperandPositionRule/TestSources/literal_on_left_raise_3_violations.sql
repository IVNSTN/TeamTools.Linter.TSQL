-- Wrong: literal on left side
SELECT 1 FROM dbo.foo WHERE '' <> foo.title

IF 1 < dbo.my_fn() RETURN

IF 0 < @var RETURN
