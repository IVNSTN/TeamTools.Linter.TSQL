-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id int not null
    , a VARCHAR(10) NULL
    , UNIQUE (a)
    , INDEX ix_tmp (id) WHERE (id <> 2)
)
GO
