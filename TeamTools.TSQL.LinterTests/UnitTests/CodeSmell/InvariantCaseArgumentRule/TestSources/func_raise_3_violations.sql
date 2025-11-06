SET @var = CASE
    WHEN RAND() > 0.5 THEN 2 -- 1
    ELSE RAND() -- 2
END

SELECT COALESCE(id, NEWID()) -- 3
FROM dbo.foo
