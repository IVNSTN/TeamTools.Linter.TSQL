if exists(
    SELECT a, b, c FROM dbo.foo
    WHERE id = 123
    EXCEPT
    SELECT d, e, f FROM dbo.bar
    WHERE parent_id = 321
)
BEGIN
    PRINT '1'
END

if exists(
    SELECT a, b, c FROM dbo.foo
    WHERE id = 123
    INTERSECT
    SELECT d, e, f FROM dbo.bar
    WHERE parent_id = 321
)
BEGIN
    PRINT '2'
END
