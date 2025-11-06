SET @var = CASE (SELECT TOP(1) id FROM dbo.foo) -- 1
    WHEN 1 THEN 2
    WHEN 2 THEN 3
    ELSE 4
END

SELECT COALESCE((SELECT TOP(1) id FROM dbo.foo), 0) -- 2

SELECT NULLIF((SELECT TOP(1) id FROM dbo.foo), 0) -- 3
