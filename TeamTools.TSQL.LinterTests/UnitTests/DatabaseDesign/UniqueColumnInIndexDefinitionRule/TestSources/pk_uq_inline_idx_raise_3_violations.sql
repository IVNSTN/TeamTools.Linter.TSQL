-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id int not null
    , a VARCHAR(10) NULL
    , CONSTRAINT PK_foo PRIMARY KEY(id, id)
    , UNIQUE (a, a)
    , INDEX ix_tmp (id, id) WHERE (id <> 2)
)
