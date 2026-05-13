DECLARE @foo TABLE (id INT)

SELECT *
FROM @bar
GO

-- table vars are not passed across batches
SELECT *
FROM @foo
