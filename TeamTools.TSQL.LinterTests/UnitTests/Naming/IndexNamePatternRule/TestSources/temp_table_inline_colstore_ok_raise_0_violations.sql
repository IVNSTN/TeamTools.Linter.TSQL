-- compatibility level min: 130
CREATE TABLE #foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
    , INDEX IUF_foo_id_o13e UNIQUE (id, open_date, title) WHERE title <> ''
);
GO

CREATE NONCLUSTERED COLUMNSTORE INDEX CL_foo_title_o18e
    ON #foo (title, open_date, is_visible);
GO

CREATE CLUSTERED COLUMNSTORE INDEX CL_foo
    ON #foo;
GO
