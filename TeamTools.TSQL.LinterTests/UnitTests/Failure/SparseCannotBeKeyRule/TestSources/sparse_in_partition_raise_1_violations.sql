CREATE TABLE dbo.foo
(
    col INT SPARSE NULL
) ON prt(col) -- here
