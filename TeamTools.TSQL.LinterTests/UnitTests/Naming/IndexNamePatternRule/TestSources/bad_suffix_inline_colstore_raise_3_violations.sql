-- compatibility level min: 130
CREATE TABLE zoo.foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
    , INDEX IU_zoo_foo_id_open_date_title UNIQUE (id, open_date, title) -- numeronim expected
);
GO

CREATE NONCLUSTERED COLUMNSTORE INDEX CL_zoo_foo_title_o555e -- wrong number in numeronim
    ON zoo.foo (title, open_date, is_visible);
GO

CREATE CLUSTERED COLUMNSTORE INDEX CL_zoo_foo_id_t26e -- no suffix expected
    ON zoo.foo;
GO
