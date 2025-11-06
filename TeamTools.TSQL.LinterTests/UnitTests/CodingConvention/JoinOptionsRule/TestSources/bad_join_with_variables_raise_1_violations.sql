DECLARE @t TABLE (id INT)
DECLARE @b TABLE (id INT)
DECLARE @z TABLE (id INT)

SELECT TOP 1 *
FROM @t t, @b b, bar
INNER JOIN @z z ON
z.id=b.id
