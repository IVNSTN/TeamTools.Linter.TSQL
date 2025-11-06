CREATE TABLE dbo.foo
(
    id    INT PRIMARY KEY,
    title VARCHAR(30),
    dt    DATETIME,
)

CREATE NONCLUSTERED INDEX IX1 ON dbo.foo(id, title) -- not unique

CREATE INDEX IX2 ON dbo.foo(dt, id, title) -- not unique
