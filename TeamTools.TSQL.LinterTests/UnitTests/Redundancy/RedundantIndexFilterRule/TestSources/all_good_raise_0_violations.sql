CREATE TABLE dbo.foo
(
    category_id INT NOT NULL
    , CONSTRAINT ck CHECK (category_id <> 1 AND category_id < 10)
)
GO

-- table name is different
CREATE INDEX ix ON dbo.bar(category_id)
WHERE category_id <> 1 AND category_id < 10
GO

-- column name is different
CREATE INDEX ix ON dbo.foo(category_id)
WHERE some_id <> 1 AND some_id < 10
GO

-- no filter
CREATE INDEX ix ON dbo.foo(category_id)
GO

-- no constraints
CREATE TABLE dbo.my_table
(
    some_id INT
)
GO

CREATE INDEX ix ON dbo.my_table(some_id)
WHERE some_id IS NOT NULL
GO

CREATE INDEX ix ON dbo.my_table(some_id)
GO
