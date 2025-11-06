SELECT t.id
    , (SELECT TOP(1) x FROM foo) -- 1
FROM dbo.tbl AS t
GO

SELECT t.id
    , (SELECT TOP(1) foo.x FROM foo)
FROM dbo.tbl AS t
WHERE NOT EXISTS(SELECT 1 FROM bar WHERE bar.id = bar_id) -- 2
