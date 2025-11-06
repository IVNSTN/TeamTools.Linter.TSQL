CREATE TABLE dbo.foo
(
    id INT,
    title VARCHAR(100),
    foo_type_id INT
)
GO

CREATE INDEX IX_foo_type_id ON dbo.foo(foo_type_id)
GO
