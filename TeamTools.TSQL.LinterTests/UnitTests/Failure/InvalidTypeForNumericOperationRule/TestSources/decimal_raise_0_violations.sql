DECLARE @a NUMERIC(10,2), @b FLOAT, @c MONEY

SET @a = @b * @c % 3;

SELECT @a / 5

SET @c /= @b
