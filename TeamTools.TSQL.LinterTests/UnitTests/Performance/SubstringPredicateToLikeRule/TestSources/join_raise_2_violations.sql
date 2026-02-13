SELECT f.id
FROM dbo.foo f
JOIN dbo.bar b
ON ((CHARINDEX('A', b.title)) = 1      -- 1
AND ('A' = LEFT(f.name, 1)))           -- 2
