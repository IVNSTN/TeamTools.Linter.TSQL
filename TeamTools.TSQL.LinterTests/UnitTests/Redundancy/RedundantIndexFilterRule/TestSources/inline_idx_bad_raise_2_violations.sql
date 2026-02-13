-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    category_id INT NOT NULL
    , CONSTRAINT ck CHECK (category_id > 0)
    , INDEX ix (category_id) WHERE
        category_id IS NOT NULL                             -- 1
        AND category_id > 0                                 -- 2
)
GO
