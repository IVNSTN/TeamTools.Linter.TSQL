-- compatibility level min: 130
CREATE TABLE zoo.foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
    , INDEX UQIDX_zoo_foo_id_o13e UNIQUE (id, open_date, title) -- IU expected
);
GO

CREATE NONCLUSTERED COLUMNSTORE INDEX IX_zoo_foo_title_o18e -- CL_ expected
    ON zoo.foo (title, open_date, is_visible);
GO

CREATE CLUSTERED COLUMNSTORE INDEX CLF_zoo_foo -- CL_ expected
    ON zoo.foo;
GO
