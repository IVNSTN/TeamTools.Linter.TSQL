CREATE TABLE #foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
);
GO

CREATE INDEX IX_foo -- no suffix
    ON #foo (open_date);
GO

CREATE NONCLUSTERED INDEX IXF_foo_ttl -- field name misspelled
    ON #foo (title)
    WHERE title IS NOT NULL;
GO

CREATE UNIQUE INDEX IU_foo_open_date_title -- fields are in wrong order
    ON #foo (title, open_date);
GO

CREATE UNIQUE INDEX IUI_foo_title_open_date_is_visible -- included fields aren't supposed to be listed
    ON #foo (title, open_date)
    INCLUDE (is_visible);
GO

CREATE UNIQUE INDEX IUIF_foo_t13e -- first field must not be included in numeronim
    ON #foo (title, open_date)
    INCLUDE (is_visible)
    WHERE title IS NOT NULL;
GO
