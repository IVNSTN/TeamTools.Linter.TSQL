CREATE TABLE dbo.foo
(
    category_id INT NOT NULL
)
GO

CREATE INDEX ix ON dbo.foo(category_id)
WHERE category_id IS NOT NULL           -- here
GO
