DECLARE @tbl dbo.my_tbl_type

INSERT @tbl(id, num)
VALUES (1, 2)

SELECT *
FROM @tbl
GO

DECLARE @tbl2 dbo.my_tbl_type

UPDATE t SET
    title = 'new title'
    OUTPUT INSERTED.title
    INTO @tbl2(title)
FROM dbo.foo t
WHERE title = 'old title'

SELECT *
FROM @tbl2
GO
