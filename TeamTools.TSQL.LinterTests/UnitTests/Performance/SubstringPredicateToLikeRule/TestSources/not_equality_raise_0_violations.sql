SELECT f.id
FROM dbo.foo f
WHERE SUBSTRING(f.title, 1, 1) BETWEEN 'A' AND 'C'
    AND LEFT(f.title) <> 'X'
