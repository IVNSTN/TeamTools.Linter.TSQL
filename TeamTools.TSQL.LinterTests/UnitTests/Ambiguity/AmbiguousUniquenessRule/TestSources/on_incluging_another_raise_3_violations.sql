CREATE TABLE dbo.foo
(
    id    INT PRIMARY KEY,
    title VARCHAR(30),
    dt    DATETIME,
)

CREATE UNIQUE NONCLUSTERED INDEX IX1 ON dbo.foo(id, title) -- 1

CREATE UNIQUE INDEX IX2 ON dbo.foo(dt, id, title) -- 2, 3 (also clashed with IX1)
