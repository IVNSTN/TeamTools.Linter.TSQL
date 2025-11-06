-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id            INT
    , title       VARCHAR(100) CONSTRAINT DF_title DEFAULT ''
    , foo_type_id INT
    , CONSTRAINT PK PRIMARY KEY (id)
    , INDEX IX_foo_type_id (foo_type_id)
    , CONSTRAINT CK CHECK (foo_type_id > 0)
);
GO
