-- compatibility level min: 110
DECLARE @str VARCHAR(3), @a CHAR(2), @b VARCHAR(1)

SET @str = IIF(@a = @b, '12345', NULL)
