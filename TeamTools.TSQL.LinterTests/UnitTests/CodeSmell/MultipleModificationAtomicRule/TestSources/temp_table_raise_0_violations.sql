INSERT #foo (a, b)
VALUES (1, 2)

UPDATE b SET
    lasdmod = GETDATE()
FROM #bar b
WHERE id = @id
GO
