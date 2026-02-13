-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    category_id INT NOT NULL
    , CONSTRAINT ck CHECK (category_id <> 1 AND category_id < 10)
    , INDEX ix1 (category_id)
    , INDEX ix2 (category_id) WHERE category_id > 100
)
GO
