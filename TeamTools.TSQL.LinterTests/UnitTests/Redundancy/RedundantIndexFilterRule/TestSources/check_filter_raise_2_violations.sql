CREATE TABLE dbo.foo
(
    category_id INT
    , CONSTRAINT ck CHECK (100 < category_id AND NOT (category_id >= (1000)))
)
GO

CREATE INDEX ix ON dbo.foo(category_id)
WHERE category_id < 1000                    -- 1
    AND category_id > 100                   -- 2
GO
