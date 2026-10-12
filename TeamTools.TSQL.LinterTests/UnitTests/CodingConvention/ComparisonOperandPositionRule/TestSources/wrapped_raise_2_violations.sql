-- Wrong: literal on left (inside parenthesis should be unwrapped)
SELECT * FROM dbo.foo WHERE (1) = foo.id

-- Wrong: variable on left (inside parenthesis should be unwrapped)
SELECT * FROM dbo.foo WHERE -(@var) = foo.value
