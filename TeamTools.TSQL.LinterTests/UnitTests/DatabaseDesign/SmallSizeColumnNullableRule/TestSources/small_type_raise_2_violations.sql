CREATE TABLE dbo.foo
(
    flag   BIT     NULL,
    -- explicit nullability attribut omitted means nullable
    volume TINYINT
)
