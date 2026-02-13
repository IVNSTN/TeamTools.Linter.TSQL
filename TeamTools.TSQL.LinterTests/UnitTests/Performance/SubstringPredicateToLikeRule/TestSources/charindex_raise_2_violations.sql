SELECT f.id
FROM dbo.foo f
WHERE (CHARINDEX('A', f.title)) = 1
    OR (1) = CHARINDEX('B', f.title)
