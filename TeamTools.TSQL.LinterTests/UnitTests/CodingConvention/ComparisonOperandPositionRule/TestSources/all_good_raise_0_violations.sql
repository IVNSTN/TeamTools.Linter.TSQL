-- Correct: function call on left, literal on right
IF dbo.my_fn() > 1 RETURN

-- Correct: function on left, variable on right
SELECT * FROM dbo.foo WHERE dbo.get_value() = -(@value)

-- Correct: complex expression on left, literal on right
SELECT * FROM dbo.foo WHERE (foo.price * 1.2) > 100

-- Correct: both sides are variables
SELECT * FROM dbo.foo WHERE foo.id = @id

-- Correct: both sides are literals
SELECT * FROM dbo.foo WHERE 1 = 1

-- both sides are variables
SELECT * FROM dbo.foo WHERE @a > @b

-- var > literal
SELECT * FROM dbo.foo WHERE @a > 0
SELECT * FROM dbo.foo WHERE @a > ((0))
