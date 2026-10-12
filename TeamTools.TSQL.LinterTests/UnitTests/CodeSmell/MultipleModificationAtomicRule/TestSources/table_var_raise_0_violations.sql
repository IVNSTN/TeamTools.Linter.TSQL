INSERT @foo (a, b)
VALUES (1, 2)

UPDATE @bar SET
    lasdmod = GETDATE()
WHERE id = @id
GO
