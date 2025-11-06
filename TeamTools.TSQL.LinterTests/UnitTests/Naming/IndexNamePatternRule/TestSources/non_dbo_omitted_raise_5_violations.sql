CREATE TABLE xxx.foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
);
GO

CREATE INDEX IX_foo_open_date
    ON zoo.foo (open_date);
GO

CREATE NONCLUSTERED INDEX IXF_foo_title
    ON zoo.foo (title)
    WHERE title IS NOT NULL;
GO

CREATE UNIQUE INDEX IU_foo_title_open_date
    ON zoo.foo (title, open_date);
GO

CREATE UNIQUE INDEX IUI_foo_title_open_date
    ON zoo.foo (title, open_date)
    INCLUDE (is_visible);
GO

CREATE UNIQUE INDEX IUIF_foo_title_open_date
    ON zoo.foo (title, open_date)
    INCLUDE (is_visible)
    WHERE title IS NOT NULL;
GO
