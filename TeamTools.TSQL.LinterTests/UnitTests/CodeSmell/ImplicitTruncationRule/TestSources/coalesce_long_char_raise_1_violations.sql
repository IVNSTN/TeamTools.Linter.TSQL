DECLARE @str VARCHAR(3), @a CHAR(2), @b VARCHAR(1)

SET @str = COALESCE(@a, '12345', @b)
