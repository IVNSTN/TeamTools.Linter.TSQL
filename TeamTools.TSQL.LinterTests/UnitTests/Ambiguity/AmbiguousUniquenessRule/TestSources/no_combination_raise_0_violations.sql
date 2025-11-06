CREATE TABLE dbo.foo
(
    id    INT PRIMARY KEY,
    title VARCHAR(30),
    dt    DATETIME,
    rv    ROWVERSION
)

CREATE UNIQUE NONCLUSTERED INDEX IX1 ON dbo.foo(title, dt)

CREATE UNIQUE INDEX IX2 ON dbo.foo(rv)
GO

DECLARE @t TABLE
(
    id    INT PRIMARY KEY,
    title VARCHAR(30),
    dt    DATETIME,
    UNIQUE (title),
    UNIQUE(dt)
)
