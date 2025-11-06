-- not inmemory
CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL
)
GO

-- no sparse cols
CREATE TABLE dbo.bar
(
    bar INT NOT NULL
) WITH (MEMORY_OPTIMIZED = ON)
GO
