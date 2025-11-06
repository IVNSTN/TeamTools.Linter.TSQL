CREATE TABLE dbo.foo
(
    title  VARCHAR(3) NULL,
    -- explicit nullability attribut omitted means nullable
    prefix NCHAR(2)
)
