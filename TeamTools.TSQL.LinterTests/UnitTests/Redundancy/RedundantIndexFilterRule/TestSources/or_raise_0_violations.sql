CREATE TABLE dbo.foo
(
    category_id INT NOT NULL
    , CONSTRAINT ck CHECK (category_id = 1 OR category_id < 10)
)
GO

CREATE INDEX ix ON dbo.foo(category_id)
WHERE category_id = 1
GO

CREATE INDEX ix ON dbo.foo(category_id)
WHERE category_id < 10
GO
