CREATE TABLE zoo.foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
);
GO

CREATE INDEX IXI_zoo_foo_open_date -- no include, so no I expected
    ON zoo.foo (open_date);
GO

CREATE NONCLUSTERED INDEX IX_zoo_foo_title -- has WHERE, so IXF_ expected
    ON zoo.foo (title)
    WHERE title IS NOT NULL;
GO

CREATE UNIQUE INDEX IX_zoo_foo_title_open_date -- for unique IU_ expected
    ON zoo.foo (title, open_date);
GO

CREATE UNIQUE INDEX IUIF_zoo_foo_title_open_date -- no filter so IUI_ expected
    ON zoo.foo (title, open_date)
    INCLUDE (is_visible);
GO

CREATE UNIQUE INDEX zoo_foo_title_open_date -- no prefix at all
    ON zoo.foo (title, open_date)
    INCLUDE (is_visible)
    WHERE title IS NOT NULL;
GO
